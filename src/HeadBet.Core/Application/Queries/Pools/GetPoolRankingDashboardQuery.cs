using Headsoft.Messaging.Abstractions.Queries;
using HeadBet.Core.Domain.Enums;
using HeadBet.Core.Domain.Interfaces;
using HeadBet.Core.Domain.Interfaces.Repositories;
using HeadBet.Core.Extensions;
using HeadBet.Core.Infrastructure.Scoring;
using HeadBet.Core.Models;

namespace HeadBet.Core.Application.Queries;

// --- Query ---
public record GetPoolRankingDashboardQuery(Guid PoolId) : QueryBase<RankingDashboardViewModel>;

// --- Handler ---
// Monta as séries do dashboard de ranking. Eixo X = cada jogo finalizado (ordem
// cronológica). Para a evolução de posição, roda o PoolRankingCalculator sobre os
// scores acumulados até cada jogo, reaproveitando a mesma regra de desempate.
public sealed class GetPoolRankingDashboardQueryHandler(
    IPoolRepository poolRepository,
    IPoolMemberRepository memberRepository,
    IUserRepository userRepository,
    IMatchRepository matchRepository,
    IMatchUserScoreRepository scoreRepository,
    IPoolScoringRuleRepository scoringRuleRepository,
    IUserContext userContext) : QueryHandlerBase<GetPoolRankingDashboardQuery, RankingDashboardViewModel>
{
    public override async Task<RankingDashboardViewModel> HandleAsync(GetPoolRankingDashboardQuery query, CancellationToken ct)
    {
        var empty = new RankingDashboardViewModel();

        var pool = await poolRepository.GetAsync(p => p.Id == query.PoolId, @readonly: true, ct);
        if (pool is null)
            return empty;

        var me = userContext.IsAuthenticated
            ? await memberRepository.GetAsync(
                m => m.PoolId == query.PoolId && m.UserId == userContext.UserId && m.Status == PoolMemberStatus.Active,
                @readonly: true, ct)
            : null;

        if (me is null && !pool.IsPublic)
            return empty;

        var activeMembers = await memberRepository.ToListAsync(
            m => m.PoolId == query.PoolId && m.Status == PoolMemberStatus.Active, @readonly: true, ct);
        if (activeMembers.Count == 0)
            return empty;

        var memberUserIds = activeMembers.Select(m => m.UserId).ToList();
        var users = await userRepository.ToListAsync(u => memberUserIds.Contains(u.Id), @readonly: true, ct);
        var userNames = users.ToDictionary(u => u.Id, u => u.Name);
        var memberTuples = activeMembers
            .Select(m => (m.UserId, UserName: userNames.GetValueOrDefault(m.UserId, string.Empty)))
            .ToList();

        // Jogos finalizados em ordem cronológica = eixo X.
        var finishedMatches = (await matchRepository.ToListAsync(
                m => m.PoolId == query.PoolId && m.Status == MatchStatus.Finished, @readonly: true, ct))
            .OrderBy(m => m.MatchDate)
            .ToList();
        if (finishedMatches.Count == 0)
            return empty;

        var n = finishedMatches.Count;
        var matchIndex = new Dictionary<Guid, int>(n);
        for (var i = 0; i < n; i++)
            matchIndex[finishedMatches[i].Id] = i;

        var scores = await scoreRepository.ToListAsync(s => s.Match.PoolId == query.PoolId, @readonly: true, ct);
        var finishedScores = scores.Where(s => matchIndex.ContainsKey(s.MatchId)).ToList();

        var exactRule = await scoringRuleRepository.GetAsync(
            r => r.PoolId == query.PoolId && r.ScoreType == ScoreType.ExactScore, @readonly: true, ct);
        var maxPerMatch = exactRule?.Points ?? 0;

        var currentUserId = me?.UserId;

        // Pontos por (usuário, índice do jogo) e o acumulado.
        var pointsByUser = memberTuples.ToDictionary(m => m.UserId, _ => new int[n]);
        foreach (var s in finishedScores)
            if (pointsByUser.TryGetValue(s.UserId, out var arr))
                arr[matchIndex[s.MatchId]] += s.Points;

        var cumulativeByUser = new Dictionary<Guid, double[]>();
        foreach (var (userId, _) in memberTuples)
        {
            var cum = new double[n];
            var running = 0;
            var per = pointsByUser[userId];
            for (var i = 0; i < n; i++)
            {
                running += per[i];
                cum[i] = running;
            }
            cumulativeByUser[userId] = cum;
        }

        // Posição após cada jogo: calculator sobre os scores acumulados até o índice i.
        var positionsByUser = memberTuples.ToDictionary(m => m.UserId, _ => new double[n]);
        for (var i = 0; i < n; i++)
        {
            var cutoff = i;
            var scoresUpTo = finishedScores.Where(s => matchIndex[s.MatchId] <= cutoff).ToList();
            var ranking = PoolRankingCalculator.Compute(memberTuples, scoresUpTo, [], pool.PrizeMode, 0m, currentUserId);
            foreach (var r in ranking)
                if (positionsByUser.TryGetValue(r.UserId, out var arr))
                    arr[i] = r.Position;
        }

        var finalRanking = PoolRankingCalculator.Compute(memberTuples, finishedScores, [], pool.PrizeMode, 0m, currentUserId);
        var finalByUser = finalRanking.ToDictionary(r => r.UserId);

        var participants = memberTuples
            .Select(m =>
            {
                var fr = finalByUser[m.UserId];
                var cumulative = cumulativeByUser[m.UserId];
                var total = cumulative.Length > 0 ? cumulative[^1] : 0;
                var efficiency = maxPerMatch > 0 ? total / (n * maxPerMatch) * 100.0 : 0;
                return new RankingDashboardParticipant
                {
                    UserId = m.UserId,
                    UserName = m.UserName,
                    IsCurrentUser = currentUserId.HasValue && m.UserId == currentUserId.Value,
                    FinalPosition = fr.Position,
                    CumulativePoints = cumulative,
                    Positions = positionsByUser[m.UserId],
                    CountsByScoreType = fr.CountsByScoreType,
                    EfficiencyPercent = Math.Round(efficiency, 1),
                };
            })
            .OrderBy(p => p.FinalPosition)
            .ThenBy(p => p.UserName)
            .ToList();

        return new RankingDashboardViewModel
        {
            HasData = true,
            MatchLabels = finishedMatches.Select(m => m.MatchDate.ToBrt().ToString("dd/MM")).ToList(),
            Participants = participants,
        };
    }
}
