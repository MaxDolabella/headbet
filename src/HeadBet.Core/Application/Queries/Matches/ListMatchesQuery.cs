using Headsoft.Core.Sorting;
using Headsoft.Messaging.Abstractions.Queries;
using HeadBet.Core.Domain.Enums;
using HeadBet.Core.Domain.Interfaces;
using HeadBet.Core.Domain.Interfaces.Repositories;
using HeadBet.Core.Extensions;
using HeadBet.Core.Models;

namespace HeadBet.Core.Application.Queries;

// --- Query ---
public record ListMatchesQuery(Guid PoolId) : QueryBase<List<MatchListViewModel>>;

// --- Handler ---
public sealed class ListMatchesQueryHandler(
    IPoolRepository poolRepository,
    IMatchRepository matchRepository,
    IBetRepository betRepository,
    IPoolMemberRepository memberRepository,
    IUserContext userContext) : QueryHandlerBase<ListMatchesQuery, List<MatchListViewModel>>
{
    public override async Task<List<MatchListViewModel>> HandleAsync(ListMatchesQuery query, CancellationToken ct)
    {
        var pool = await poolRepository.GetAsync(p => p.Id == query.PoolId, @readonly: true, ct);
        if (pool is null)
            return [];

        var userId = userContext.UserId;
        var isMember = userId != Guid.Empty && await memberRepository.AnyAsync(
            m => m.PoolId == query.PoolId
                 && m.UserId == userId
                 && m.Status == PoolMemberStatus.Active,
            ct);

        if (!isMember && !pool.IsPublic)
            return [];

        var matches = await matchRepository.ToListAsync<MatchListViewModel>(
            m => m.PoolId == query.PoolId,
            null,
            m => m.MatchDate,
            SortDirection.Ascending,
            ct);

        if (isMember && matches.Count > 0)
        {
            var matchIds = matches.Select(m => m.Id).ToHashSet();
            var betsByMatch = (await betRepository.ToListAsync(
                    b => b.UserId == userId && matchIds.Contains(b.MatchId),
                    @readonly: true, ct))
                .ToDictionary(b => b.MatchId);

            foreach (var m in matches)
            {
                if (!betsByMatch.TryGetValue(m.Id, out var bet)) continue;
                m.HasBet = true;
                m.BetHomeScore = bet.HomeScore;
                m.BetAwayScore = bet.AwayScore;
            }
        }

        foreach (var m in matches)
            m.MatchDate = m.MatchDate.ToBrt();

        return matches;
    }
}
