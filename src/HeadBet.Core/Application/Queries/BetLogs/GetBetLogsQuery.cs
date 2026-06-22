using Headsoft.Messaging.Abstractions.Queries;
using HeadBet.Core.Domain.Interfaces.Repositories;
using HeadBet.Core.Extensions;
using HeadBet.Core.Models;

namespace HeadBet.Core.Application.Queries;

// --- Query ---
// Log de palpites para a tela de auditoria (só superadmin). Filtros opcionais por bolão/usuário.
public record GetBetLogsQuery(Guid? PoolId, Guid? UserId) : QueryBase<List<BetLogItemViewModel>>;

// --- Handler ---
public sealed class GetBetLogsQueryHandler(
    IBetLogRepository betLogRepository,
    IUserRepository userRepository,
    IPoolRepository poolRepository,
    IMatchRepository matchRepository,
    ITeamRepository teamRepository) : QueryHandlerBase<GetBetLogsQuery, List<BetLogItemViewModel>>
{
    private const int MAX_ROWS = 500;

    public override async Task<List<BetLogItemViewModel>> HandleAsync(GetBetLogsQuery query, CancellationToken ct)
    {
        var logs = (await betLogRepository.ToListAsync(
                l => (query.PoolId == null || l.PoolId == query.PoolId)
                     && (query.UserId == null || l.UserId == query.UserId),
                @readonly: true, ct))
            .OrderByDescending(l => l.CreatedAt)
            .Take(MAX_ROWS)
            .ToList();

        if (logs.Count == 0)
            return [];

        var userIds = logs.Select(l => l.UserId).Distinct().ToList();
        var poolIds = logs.Select(l => l.PoolId).Distinct().ToList();
        var matchIds = logs.Select(l => l.MatchId).Distinct().ToList();

        var userNames = (await userRepository.ToListAsync(u => userIds.Contains(u.Id), @readonly: true, ct))
            .ToDictionary(u => u.Id, u => u.Name);
        var poolNames = (await poolRepository.ToListAsync(p => poolIds.Contains(p.Id), @readonly: true, ct))
            .ToDictionary(p => p.Id, p => p.Name);
        var matches = await matchRepository.ToListAsync(m => matchIds.Contains(m.Id), @readonly: true, ct);

        var teamIds = matches.SelectMany(m => new[] { m.HomeTeamId, m.AwayTeamId }).Distinct().ToList();
        var teamNames = (await teamRepository.ToListAsync(t => teamIds.Contains(t.Id), @readonly: true, ct))
            .ToDictionary(t => t.Id, t => t.Name);

        var matchLabels = matches.ToDictionary(
            m => m.Id,
            m => $"{teamNames.GetValueOrDefault(m.HomeTeamId, "?")} x {teamNames.GetValueOrDefault(m.AwayTeamId, "?")}");

        return logs.Select(l => new BetLogItemViewModel
        {
            PoolId = l.PoolId,
            UserId = l.UserId,
            CreatedAt = l.CreatedAt.ToBrt(),
            PoolName = poolNames.GetValueOrDefault(l.PoolId, string.Empty),
            MatchLabel = matchLabels.GetValueOrDefault(l.MatchId, string.Empty),
            UserName = userNames.GetValueOrDefault(l.UserId, string.Empty),
            Action = l.Action,
            OldHomeScore = l.OldHomeScore,
            OldAwayScore = l.OldAwayScore,
            NewHomeScore = l.NewHomeScore,
            NewAwayScore = l.NewAwayScore,
        }).ToList();
    }
}
