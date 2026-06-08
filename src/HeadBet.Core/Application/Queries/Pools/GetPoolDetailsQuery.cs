using AutoMapper;
using Headsoft.Core.Sorting;
using Headsoft.Messaging.Abstractions.Queries;
using HeadBet.Core.Domain.Enums;
using HeadBet.Core.Domain.Interfaces;
using HeadBet.Core.Domain.Interfaces.Repositories;
using HeadBet.Core.Extensions;
using HeadBet.Core.Infrastructure.Scoring;
using HeadBet.Core.Models;

namespace HeadBet.Core.Application.Queries;

// --- Query ---
public record GetPoolDetailsQuery(Guid Id) : QueryBase<PoolDetailsViewModel?>;

// --- Handler ---
public sealed class GetPoolDetailsQueryHandler(
    IPoolRepository poolRepository,
    IPoolMemberRepository memberRepository,
    IPoolScoringRuleRepository scoringRepository,
    IPoolPrizeRepository prizeRepository,
    IMatchRepository matchRepository,
    IUserRepository userRepository,
    IMatchUserScoreRepository scoreRepository,
    IMapper mapper,
    IUserContext userContext)
    : QueryHandlerBase<GetPoolDetailsQuery, PoolDetailsViewModel?>
{
    private const int MATCH_PREVIEW_LIMIT = 5;

    public override async Task<PoolDetailsViewModel?> HandleAsync(GetPoolDetailsQuery query, CancellationToken ct)
    {
        var userId = userContext.UserId;

        var pool = await poolRepository.GetAsync(p => p.Id == query.Id, @readonly: true, ct);
        if (pool is null)
            return null;

        var me = await memberRepository.GetAsync(
            m => m.PoolId == query.Id && m.UserId == userId && m.Status == PoolMemberStatus.Active,
            @readonly: true, ct);

        if (me is null && !pool.IsPublic)
            return null;

        var vm = mapper.Map<PoolDetailsViewModel>(pool);
        vm.MyRole = me?.Role;

        vm.ScoringRules = await scoringRepository.ToListAsync<ScoringRuleItemViewModel>(r => r.PoolId == query.Id, ct);

        vm.Prizes = await prizeRepository.ToListAsync<PrizeItemViewModel>(
            p => p.PoolId == query.Id, null, p => p.Position, SortDirection.Ascending, ct);

        var activeMemberCount = await memberRepository.CountAsync(
            m => m.PoolId == query.Id && m.Status == PoolMemberStatus.Active, ct);

        var collected = pool.CollectedAmount ?? (pool.EntryFee ?? 0m) * activeMemberCount;
        vm.CollectedAmount = collected;
        foreach (var prize in vm.Prizes)
        {
            prize.CalculatedAmount = pool.PrizeMode == PrizeMode.Fixed
                ? prize.FixedAmount ?? 0m
                : collected * (prize.Percentage ?? 0m) / 100m;
        }

        vm.Members = await memberRepository.ToListAsync<PoolMemberItemViewModel>(
            m => m.PoolId == query.Id, null, m => m.JoinedAt, SortDirection.Ascending, ct);

        foreach (var m in vm.Members)
            m.JoinedAt = m.JoinedAt.ToBrt();

        var now = DateTime.UtcNow;

        var upcoming = await matchRepository.ToListAsync<MatchListViewModel>(
            m => m.PoolId == query.Id && m.MatchDate >= now && m.Status != MatchStatus.Cancelled,
            null,
            m => m.MatchDate,
            SortDirection.Ascending,
            ct);

        vm.UpcomingMatches = upcoming.Take(MATCH_PREVIEW_LIMIT).ToList();
        foreach (var m in vm.UpcomingMatches)
            m.MatchDate = m.MatchDate.ToBrt();

        var finished = await matchRepository.ToListAsync<MatchListViewModel>(
            m => m.PoolId == query.Id && m.Status == MatchStatus.Finished,
            null,
            m => m.MatchDate,
            SortDirection.Descending,
            ct);

        vm.FinishedMatches = finished.Take(MATCH_PREVIEW_LIMIT).ToList();
        foreach (var m in vm.FinishedMatches)
            m.MatchDate = m.MatchDate.ToBrt();

        var activeMembers = vm.Members
            .Where(m => m.Status == PoolMemberStatus.Active)
            .ToList();
        var memberUserIds = activeMembers.Select(m => m.UserId).ToList();

        var users = await userRepository.ToListAsync(
            u => memberUserIds.Contains(u.Id), @readonly: true, ct);
        var userNames = users.ToDictionary(u => u.Id, u => u.Name);

        var memberTuples = activeMembers
            .Select(m => (m.UserId, UserName: userNames.GetValueOrDefault(m.UserId, string.Empty)))
            .ToList();

        var scores = await scoreRepository.ToListAsync(
            s => s.Match.PoolId == query.Id, @readonly: true, ct);

        var prizesEntities = await prizeRepository.ToListAsync(
            p => p.PoolId == query.Id, @readonly: true, ct);

        vm.Ranking = PoolRankingCalculator.Compute(
            memberTuples,
            scores,
            prizesEntities,
            pool.PrizeMode,
            collected,
            me?.UserId);

        return vm;
    }
}
