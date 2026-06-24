using Headsoft.Messaging.Abstractions.Queries;
using HeadBet.Core.Domain.Entities;
using HeadBet.Core.Domain.Enums;
using HeadBet.Core.Domain.Interfaces;
using HeadBet.Core.Domain.Interfaces.Repositories;
using HeadBet.Core.Extensions;
using HeadBet.Core.Models;

namespace HeadBet.Core.Application.Queries;

// --- Query ---
// Palpites de um participante em jogos finalizados. Pública quando o bolão é público.
public record GetUserBetsQuery(Guid PoolId, Guid UserId) : QueryBase<UserBetsViewModel>;

// --- Handler ---
public sealed class GetUserBetsQueryHandler(
    IPoolRepository poolRepository,
    IPoolMemberRepository memberRepository,
    IMatchRepository matchRepository,
    IBetRepository betRepository,
    IMatchUserScoreRepository scoreRepository,
    IUserRepository userRepository,
    ITeamRepository teamRepository,
    IUserContext userContext) : QueryHandlerBase<GetUserBetsQuery, UserBetsViewModel>
{
    public override async Task<UserBetsViewModel> HandleAsync(GetUserBetsQuery query, CancellationToken ct)
    {
        var pool = await poolRepository.GetAsync(p => p.Id == query.PoolId, @readonly: true, ct);
        if (pool is null)
            return new UserBetsViewModel();

        var viewerIsMember = userContext.IsAuthenticated && await memberRepository.AnyAsync(
            m => m.PoolId == query.PoolId
                 && m.UserId == userContext.UserId
                 && m.Status == PoolMemberStatus.Active,
            ct);

        if (!viewerIsMember && !pool.IsPublic)
            return new UserBetsViewModel { PoolId = pool.Id, PoolName = pool.Name, HasAccess = false };

        var result = new UserBetsViewModel
        {
            PoolId = pool.Id,
            PoolName = pool.Name,
            UserId = query.UserId,
            IsCurrentUser = userContext.IsAuthenticated && userContext.UserId == query.UserId,
            HasAccess = true,
        };

        // O alvo precisa ser membro (qualquer status) do bolão.
        var targetMember = await memberRepository.AnyAsync(
            m => m.PoolId == query.PoolId && m.UserId == query.UserId, ct);
        if (!targetMember)
            return result; // sem nome/sem palpites

        var user = await userRepository.GetAsync(u => u.Id == query.UserId, @readonly: true, ct);
        result.UserName = user?.Name ?? string.Empty;

        var matches = await matchRepository.ToListAsync(
            m => m.PoolId == query.PoolId && m.Status == MatchStatus.Finished, @readonly: true, ct);
        if (matches.Count == 0)
            return result;

        var matchIds = matches.Select(m => m.Id).ToHashSet();

        var bets = await betRepository.ToListAsync(
            b => b.UserId == query.UserId && matchIds.Contains(b.MatchId), @readonly: true, ct);
        var scores = await scoreRepository.ToListAsync(
            s => s.UserId == query.UserId && matchIds.Contains(s.MatchId), @readonly: true, ct);

        var teamIds = matches.SelectMany(m => new[] { m.HomeTeamId, m.AwayTeamId }).Distinct().ToList();
        var teamNames = (await teamRepository.ToListAsync(t => teamIds.Contains(t.Id), @readonly: true, ct))
            .ToDictionary(t => t.Id, t => t.Name);

        string Label(Match m) => $"{teamNames.GetValueOrDefault(m.HomeTeamId, "?")} x {teamNames.GetValueOrDefault(m.AwayTeamId, "?")}";
        string ResultLabel(Match m) => m.HomeScore.HasValue && m.AwayScore.HasValue ? $"{m.HomeScore}x{m.AwayScore}" : "—";

        var matchById = matches.ToDictionary(m => m.Id);
        var scoreByMatch = scores.ToDictionary(s => s.MatchId);

        result.Bets = bets
            .Where(b => scoreByMatch.ContainsKey(b.MatchId))
            .Select(b =>
            {
                var m = matchById[b.MatchId];
                var score = scoreByMatch[b.MatchId];
                return new StatBetItemViewModel
                {
                    MatchId = b.MatchId,
                    MatchLabel = Label(m),
                    MatchDate = m.MatchDate.ToBrt(),
                    ResultLabel = ResultLabel(m),
                    UserName = result.UserName,
                    BetLabel = $"{b.HomeScore}x{b.AwayScore}",
                    Points = score.Points,
                    AppliedRule = score.AppliedRule,
                };
            })
            .OrderByDescending(x => x.MatchDate)
            .ToList();

        result.BetCount = result.Bets.Count;
        result.TotalPoints = result.Bets.Sum(x => x.Points);

        return result;
    }
}
