using Headsoft.Messaging.Abstractions.Queries;
using HeadBet.Core.Domain.Entities;
using HeadBet.Core.Domain.Enums;
using HeadBet.Core.Domain.Interfaces;
using HeadBet.Core.Domain.Interfaces.Repositories;
using HeadBet.Core.Extensions;
using HeadBet.Core.Models;

namespace HeadBet.Core.Application.Queries;

// --- Query ---
// Estatísticas do bolão centradas em jogo/palpite, só de jogos finalizados.
// Pública quando o bolão é público (espelha o guard do resumo de ranking).
public record GetPoolStatsQuery(Guid PoolId) : QueryBase<PoolStatsViewModel>;

// --- Handler ---
public sealed class GetPoolStatsQueryHandler(
    IPoolRepository poolRepository,
    IPoolMemberRepository memberRepository,
    IMatchRepository matchRepository,
    IBetRepository betRepository,
    IMatchUserScoreRepository scoreRepository,
    IUserRepository userRepository,
    ITeamRepository teamRepository,
    IUserContext userContext) : QueryHandlerBase<GetPoolStatsQuery, PoolStatsViewModel>
{
    private const int TOP_N = 5;
    private const int CONSENSUS_TOP = 3;

    public override async Task<PoolStatsViewModel> HandleAsync(GetPoolStatsQuery query, CancellationToken ct)
    {
        var pool = await poolRepository.GetAsync(p => p.Id == query.PoolId, @readonly: true, ct);
        if (pool is null)
            return new PoolStatsViewModel();

        var isMember = userContext.IsAuthenticated && await memberRepository.AnyAsync(
            m => m.PoolId == query.PoolId
                 && m.UserId == userContext.UserId
                 && m.Status == PoolMemberStatus.Active,
            ct);

        if (!isMember && !pool.IsPublic)
            return new PoolStatsViewModel { PoolId = pool.Id, PoolName = pool.Name, HasData = false };

        var result = new PoolStatsViewModel { PoolId = pool.Id, PoolName = pool.Name };

        var matches = await matchRepository.ToListAsync(
            m => m.PoolId == query.PoolId && m.Status == MatchStatus.Finished, @readonly: true, ct);

        if (matches.Count == 0)
            return result; // HasData = false

        var matchIds = matches.Select(m => m.Id).ToHashSet();

        var bets = await betRepository.ToListAsync(b => matchIds.Contains(b.MatchId), @readonly: true, ct);
        var scores = await scoreRepository.ToListAsync(s => matchIds.Contains(s.MatchId), @readonly: true, ct);

        var userIds = bets.Select(b => b.UserId).Distinct().ToList();
        var userNames = (await userRepository.ToListAsync(u => userIds.Contains(u.Id), @readonly: true, ct))
            .ToDictionary(u => u.Id, u => u.Name);

        var teamIds = matches.SelectMany(m => new[] { m.HomeTeamId, m.AwayTeamId }).Distinct().ToList();
        var teamNames = (await teamRepository.ToListAsync(t => teamIds.Contains(t.Id), @readonly: true, ct))
            .ToDictionary(t => t.Id, t => t.Name);

        string Label(Match m) => $"{teamNames.GetValueOrDefault(m.HomeTeamId, "?")} x {teamNames.GetValueOrDefault(m.AwayTeamId, "?")}";
        string Result(Match m) => m.HomeScore.HasValue && m.AwayScore.HasValue ? $"{m.HomeScore}x{m.AwayScore}" : "—";

        var matchById = matches.ToDictionary(m => m.Id);
        var scoreByKey = scores.ToDictionary(s => (s.MatchId, s.UserId));
        var betByKey = bets.ToDictionary(b => (b.MatchId, b.UserId));
        var betsByMatch = bets.GroupBy(b => b.MatchId).ToDictionary(g => g.Key, g => g.ToList());

        result.HasData = true;

        // --- Bloco 1: lista de palpites (só os que têm score; usuário inativo é ignorado) ---
        result.Bets = bets
            .Where(b => scoreByKey.ContainsKey((b.MatchId, b.UserId)))
            .Select(b =>
            {
                var m = matchById[b.MatchId];
                var score = scoreByKey[(b.MatchId, b.UserId)];
                return new StatBetItemViewModel
                {
                    MatchId = b.MatchId,
                    MatchLabel = Label(m),
                    MatchDate = m.MatchDate.ToBrt(),
                    ResultLabel = Result(m),
                    UserName = userNames.GetValueOrDefault(b.UserId, string.Empty),
                    BetLabel = $"{b.HomeScore}x{b.AwayScore}",
                    Points = score.Points,
                    AppliedRule = score.AppliedRule,
                };
            })
            .OrderByDescending(x => x.MatchDate)
            .ThenByDescending(x => x.Points)
            .ToList();

        result.Users = result.Bets
            .Select(b => b.UserName)
            .Where(n => !string.IsNullOrWhiteSpace(n))
            .Distinct()
            .OrderBy(n => n)
            .ToList();

        // --- Bloco 2: destaques de jogos ---
        // Mais cravadas (nº de ExactScore por jogo).
        result.MostExact = scores
            .Where(s => s.AppliedRule == ScoreType.ExactScore)
            .GroupBy(s => s.MatchId)
            .Select(g => new GameHighlightViewModel
            {
                MatchId = g.Key,
                MatchLabel = Label(matchById[g.Key]),
                ResultLabel = Result(matchById[g.Key]),
                Count = g.Count(),
            })
            .OrderByDescending(x => x.Count)
            .ThenBy(x => x.MatchLabel)
            .Take(TOP_N)
            .ToList();

        // Média de pontos entre quem palpitou, por jogo.
        var averages = betsByMatch
            .Select(kvp =>
            {
                var m = matchById[kvp.Key];
                var pts = kvp.Value
                    .Where(b => scoreByKey.ContainsKey((b.MatchId, b.UserId)))
                    .Select(b => scoreByKey[(b.MatchId, b.UserId)].Points)
                    .ToList();
                return new GameHighlightViewModel
                {
                    MatchId = m.Id,
                    MatchLabel = Label(m),
                    ResultLabel = Result(m),
                    Count = pts.Count,
                    Average = pts.Count > 0 ? pts.Average() : 0d,
                };
            })
            .Where(x => x.Count > 0)
            .ToList();

        result.TopAverage = averages
            .OrderByDescending(x => x.Average).ThenBy(x => x.MatchLabel).Take(TOP_N).ToList();
        result.BottomAverage = averages
            .OrderBy(x => x.Average).ThenBy(x => x.MatchLabel).Take(TOP_N).ToList();

        // --- Bloco 3: consenso por jogo ---
        // Para cada jogo, mede o maior nº de palpites iguais num mesmo placar (a repetição).
        // Depois agrupa por esse valor e mostra os 3 maiores: cada valor traz os jogos onde
        // a repetição máxima foi exatamente aquela (independente do placar).
        var perGame = betsByMatch
            .Select(kvp =>
            {
                var m = matchById[kvp.Key];
                var top = kvp.Value
                    .GroupBy(b => (b.HomeScore, b.AwayScore))
                    .Select(g => (Label: $"{g.Key.HomeScore}x{g.Key.AwayScore}", Count: g.Count()))
                    .OrderByDescending(g => g.Count)
                    .ThenBy(g => g.Label)
                    .First();

                var exact = m.HomeScore.HasValue && m.AwayScore.HasValue
                    ? kvp.Value.Count(b => b.HomeScore == m.HomeScore && b.AwayScore == m.AwayScore)
                    : 0;

                return (top.Count, Game: new ConsensusGameViewModel
                {
                    MatchId = m.Id,
                    MatchLabel = Label(m),
                    MatchDate = m.MatchDate.ToBrt(),
                    ResultLabel = Result(m),
                    ConsensusLabel = top.Label,
                    ExactCount = exact,
                });
            })
            .ToList();

        result.Consensus = perGame
            .GroupBy(x => x.Count)
            .Select(g => new ConsensusGroupViewModel
            {
                RepeatCount = g.Key,
                GameCount = g.Count(),
                Games = g.Select(x => x.Game)
                    .OrderByDescending(x => x.MatchDate)
                    .ThenBy(x => x.MatchLabel)
                    .ToList(),
            })
            .OrderByDescending(x => x.RepeatCount)
            .Take(CONSENSUS_TOP)
            .ToList();

        // --- Bloco 4: "lobo solitário" ---
        // Considera só quem palpitou (AppliedRule != NoBet). Em cada jogo:
        //   SoloHit  -> exatamente 1 cravou (ExactScore) e TODOS os outros erraram o resultado (Consolation).
        //   SoloMiss -> exatamente 1 errou o resultado (Consolation) e TODOS os outros cravaram (ExactScore).
        SoloMatchViewModel BuildSolo(Match m, MatchUserScore lone, int othersCount) => new()
        {
            MatchId = m.Id,
            MatchLabel = Label(m),
            MatchDate = m.MatchDate.ToBrt(),
            ResultLabel = Result(m),
            UserName = userNames.GetValueOrDefault(lone.UserId, string.Empty),
            BetLabel = betByKey.TryGetValue((m.Id, lone.UserId), out var b) ? $"{b.HomeScore}x{b.AwayScore}" : "—",
            OthersCount = othersCount,
        };

        var scoresByMatch = scores
            .Where(s => s.AppliedRule != ScoreType.NoBet)
            .GroupBy(s => s.MatchId)
            .ToList();

        foreach (var g in scoresByMatch)
        {
            var m = matchById[g.Key];
            var exact = g.Where(s => s.AppliedRule == ScoreType.ExactScore).ToList();
            var consolation = g.Where(s => s.AppliedRule == ScoreType.Consolation).ToList();
            var others = g.Count() - 1;

            // Só um cravou (independente dos outros).
            if (exact.Count == 1 && others >= 1)
            {
                result.SoloHitLoose.Add(BuildSolo(m, exact[0], others));

                // Versão estrita: todo o resto caiu na consolação.
                if (consolation.Count == others)
                    result.SoloHit.Add(BuildSolo(m, exact[0], others));
            }

            // Só um errou o resultado (os outros acertaram ao menos parcialmente, pois bettors exclui NoBet).
            if (consolation.Count == 1 && others >= 1)
            {
                result.SoloMissLoose.Add(BuildSolo(m, consolation[0], others));

                // Versão estrita: todo o resto cravou.
                if (exact.Count == others)
                    result.SoloMiss.Add(BuildSolo(m, consolation[0], others));
            }
        }

        result.SoloHit = result.SoloHit.OrderByDescending(x => x.MatchDate).Take(TOP_N).ToList();
        result.SoloHitLoose = result.SoloHitLoose.OrderByDescending(x => x.MatchDate).Take(TOP_N).ToList();
        result.SoloMiss = result.SoloMiss.OrderByDescending(x => x.MatchDate).Take(TOP_N).ToList();
        result.SoloMissLoose = result.SoloMissLoose.OrderByDescending(x => x.MatchDate).Take(TOP_N).ToList();

        return result;
    }
}
