using Headsoft.Core.Sorting;
using Headsoft.Messaging.Abstractions.Queries;
using HeadBet.Core.Domain.Enums;
using HeadBet.Core.Domain.Interfaces;
using HeadBet.Core.Domain.Interfaces.Repositories;
using HeadBet.Core.Extensions;
using HeadBet.Core.Models;

namespace HeadBet.Core.Application.Queries;

// --- Query ---
public record ListPoolBetsQuery(Guid PoolId) : QueryBase<PoolBetsViewModel?>;

// --- Handler ---
public sealed class ListPoolBetsQueryHandler(
    IPoolRepository poolRepository,
    IPoolMemberRepository memberRepository,
    IMatchRepository matchRepository,
    IBetRepository betRepository,
    IUserContext userContext) : QueryHandlerBase<ListPoolBetsQuery, PoolBetsViewModel?>
{
    private const int BET_CUTOFF_MINUTES = 2;

    public override async Task<PoolBetsViewModel?> HandleAsync(ListPoolBetsQuery query, CancellationToken ct)
    {
        var userId = userContext.UserId;

        var me = await memberRepository.GetAsync(
            m => m.PoolId == query.PoolId
                 && m.UserId == userId
                 && m.Status == PoolMemberStatus.Active,
            @readonly: true,
            ct);

        if (me is null)
            return null;

        var pool = await poolRepository.GetAsync(p => p.Id == query.PoolId, @readonly: true, ct);
        if (pool is null)
            return null;

        var isAdmin = me.Role == PoolMemberRole.Admin || userContext.IsAdmin;

        var cutoff = DateTime.UtcNow.AddMinutes(BET_CUTOFF_MINUTES);

        var items = await matchRepository.ToListAsync<BetItemViewModel>(
            m => m.PoolId == query.PoolId && m.MatchDate > cutoff,
            null,
            m => m.MatchDate,
            SortDirection.Ascending,
            ct);

        if (items.Count == 0)
            return new PoolBetsViewModel { PoolId = pool.Id, PoolName = pool.Name, IsAdmin = isAdmin };

        var matchIds = items.Select(i => i.MatchId).ToList();

        var existingBets = await betRepository.ToListAsync(
            b => b.UserId == userId && matchIds.Contains(b.MatchId),
            @readonly: true,
            ct);

        var betByMatch = existingBets.ToDictionary(b => b.MatchId);

        foreach (var item in items)
        {
            item.MatchDate = item.MatchDate.ToBrt();

            if (betByMatch.TryGetValue(item.MatchId, out var bet))
            {
                item.HomeScore = bet.HomeScore;
                item.AwayScore = bet.AwayScore;
            }
        }

        return new PoolBetsViewModel
        {
            PoolId = pool.Id,
            PoolName = pool.Name,
            IsAdmin = isAdmin,
            Items = items,
        };
    }
}
