using Headsoft.Messaging.Abstractions.Queries;
using HeadBet.Core.Domain.Enums;
using HeadBet.Core.Domain.Interfaces;
using HeadBet.Core.Domain.Interfaces.Repositories;
using HeadBet.Core.Infrastructure.Scoring;
using HeadBet.Core.Models;

namespace HeadBet.Core.Application.Queries;

// --- Query ---
public record GetPoolRankingQuery(Guid PoolId) : QueryBase<PoolRankingViewModel?>;

// --- Handler ---
public sealed class GetPoolRankingQueryHandler(
    IPoolRepository poolRepository,
    IPoolMemberRepository memberRepository,
    IUserRepository userRepository,
    IMatchUserScoreRepository scoreRepository,
    IPoolPrizeRepository prizeRepository,
    IUserContext userContext) : QueryHandlerBase<GetPoolRankingQuery, PoolRankingViewModel?>
{
    public override async Task<PoolRankingViewModel?> HandleAsync(GetPoolRankingQuery query, CancellationToken ct)
    {
        var pool = await poolRepository.GetAsync(p => p.Id == query.PoolId, @readonly: true, ct);
        if (pool is null)
            return null;

        var me = userContext.IsAuthenticated
            ? await memberRepository.GetAsync(
                m => m.PoolId == query.PoolId
                     && m.UserId == userContext.UserId
                     && m.Status == PoolMemberStatus.Active,
                @readonly: true, ct)
            : null;

        if (me is null && !pool.IsPublic)
            return null;

        var activeMembers = await memberRepository.ToListAsync(
            m => m.PoolId == query.PoolId && m.Status == PoolMemberStatus.Active,
            @readonly: true, ct);

        var memberUserIds = activeMembers.Select(m => m.UserId).ToList();

        var users = await userRepository.ToListAsync(
            u => memberUserIds.Contains(u.Id), @readonly: true, ct);

        var userNames = users.ToDictionary(u => u.Id, u => u.Name);

        var memberTuples = activeMembers
            .Select(m => (m.UserId, UserName: userNames.GetValueOrDefault(m.UserId, string.Empty)))
            .ToList();

        var scores = await scoreRepository.ToListAsync(
            s => s.Match.PoolId == query.PoolId, @readonly: true, ct);

        var prizes = await prizeRepository.ToListAsync(
            p => p.PoolId == query.PoolId, @readonly: true, ct);

        var currentUserId = me?.UserId;

        var items = PoolRankingCalculator.Compute(
            memberTuples,
            scores,
            prizes,
            pool.PrizeMode,
            pool.EntryFee,
            currentUserId);

        return new PoolRankingViewModel
        {
            PoolId = pool.Id,
            PoolName = pool.Name,
            IsPaid = pool.IsPaid,
            PrizeMode = pool.PrizeMode,
            IsAnonymousView = !userContext.IsAuthenticated,
            IsMember = me is not null,
            Items = items,
        };
    }
}
