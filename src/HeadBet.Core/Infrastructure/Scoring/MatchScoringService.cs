using Headsoft.Core.Helpers;
using Headsoft.Core.Interfaces.Data;
using HeadBet.Core.Domain.Entities;
using HeadBet.Core.Domain.Enums;
using HeadBet.Core.Domain.Interfaces;
using HeadBet.Core.Domain.Interfaces.Repositories;

namespace HeadBet.Core.Infrastructure.Scoring;

public sealed class MatchScoringService(
    IMatchRepository matchRepository,
    IPoolMemberRepository memberRepository,
    IPoolScoringRuleRepository scoringRuleRepository,
    IBetRepository betRepository,
    IMatchUserScoreRepository scoreRepository,
    IUnitOfWork uow) : IMatchScoringService
{
    public async Task RecomputeForMatchAsync(Guid matchId, CancellationToken ct)
    {
        var match = await matchRepository.GetAsync(m => m.Id == matchId, @readonly: true, ct);
        if (match is null)
            return;

        var rules = await LoadRulesAsync(match.PoolId, ct);
        var activeUserIds = await LoadActiveUserIdsAsync(match.PoolId, ct);
        var bets = await betRepository.ToListAsync(b => b.MatchId == matchId, @readonly: true, ct);
        var existingScores = await scoreRepository.ToListAsync(s => s.MatchId == matchId, @readonly: false, ct);

        var betsByUser = bets.ToDictionary(b => b.UserId);
        var scoresByUser = existingScores.ToDictionary(s => s.UserId);
        var now = DateTime.UtcNow;

        foreach (var userId in activeUserIds)
        {
            betsByUser.TryGetValue(userId, out var bet);
            var (points, appliedRule) = ComputePoints(bet, match, rules);

            if (scoresByUser.TryGetValue(userId, out var score))
            {
                score.Points = points;
                score.AppliedRule = appliedRule;
                score.UpdatedAt = now;
            }
            else
            {
                await scoreRepository.AddAsync(new MatchUserScore
                {
                    Id = UIDGen.NewGuid(),
                    MatchId = matchId,
                    UserId = userId,
                    Points = points,
                    AppliedRule = appliedRule,
                    UpdatedAt = now,
                }, ct);
            }
        }

        var activeSet = activeUserIds.ToHashSet();
        foreach (var orphan in existingScores.Where(s => !activeSet.Contains(s.UserId)))
            await scoreRepository.DeleteAsync([orphan.Id], ct);

        await uow.SaveChangesAsync(ct);
    }

    public async Task RecomputeForUserBetAsync(Guid matchId, Guid userId, CancellationToken ct)
    {
        var match = await matchRepository.GetAsync(m => m.Id == matchId, @readonly: true, ct);
        if (match is null)
            return;

        var isActiveMember = await memberRepository.AnyAsync(
            m => m.PoolId == match.PoolId && m.UserId == userId && m.Status == PoolMemberStatus.Active, ct);

        if (!isActiveMember)
            return;

        var rules = await LoadRulesAsync(match.PoolId, ct);
        var bet = await betRepository.GetAsync(
            b => b.MatchId == matchId && b.UserId == userId, @readonly: true, ct);

        var (points, appliedRule) = ComputePoints(bet, match, rules);
        var now = DateTime.UtcNow;

        var score = await scoreRepository.GetAsync(
            s => s.MatchId == matchId && s.UserId == userId, @readonly: false, ct);

        if (score is null)
        {
            await scoreRepository.AddAsync(new MatchUserScore
            {
                Id = UIDGen.NewGuid(),
                MatchId = matchId,
                UserId = userId,
                Points = points,
                AppliedRule = appliedRule,
                UpdatedAt = now,
            }, ct);
        }
        else
        {
            score.Points = points;
            score.AppliedRule = appliedRule;
            score.UpdatedAt = now;
        }

        await uow.SaveChangesAsync(ct);
    }

    private async Task<Dictionary<ScoreType, int>> LoadRulesAsync(Guid poolId, CancellationToken ct)
    {
        var rules = await scoringRuleRepository.ToListAsync(r => r.PoolId == poolId, @readonly: true, ct);
        return rules.ToDictionary(r => r.ScoreType, r => r.Points);
    }

    private async Task<List<Guid>> LoadActiveUserIdsAsync(Guid poolId, CancellationToken ct)
    {
        var members = await memberRepository.ToListAsync(
            m => m.PoolId == poolId && m.Status == PoolMemberStatus.Active, @readonly: true, ct);
        return members.Select(m => m.UserId).ToList();
    }

    internal static (int points, ScoreType appliedRule) ComputePoints(
        Bet? bet, Match match, Dictionary<ScoreType, int> rules)
    {
        if (match.Status == MatchStatus.Cancelled
            || !match.HomeScore.HasValue || !match.AwayScore.HasValue)
            return (0, ScoreType.NoBet);

        if (bet is null)
            return (rules.GetValueOrDefault(ScoreType.NoBet), ScoreType.NoBet);

        var realHome = match.HomeScore.Value;
        var realAway = match.AwayScore.Value;
        var betHome = bet.HomeScore;
        var betAway = bet.AwayScore;

        if (betHome == realHome && betAway == realAway)
            return (rules.GetValueOrDefault(ScoreType.ExactScore), ScoreType.ExactScore);

        var realOutcome = Math.Sign(realHome - realAway);
        var betOutcome = Math.Sign(betHome - betAway);
        var sameOutcome = realOutcome == betOutcome;

        if (sameOutcome)
        {
            if (realOutcome == 0)
                return (rules.GetValueOrDefault(ScoreType.WinnerAndDifference), ScoreType.WinnerAndDifference);

            if ((realHome - realAway) == (betHome - betAway))
                return (rules.GetValueOrDefault(ScoreType.WinnerAndDifference), ScoreType.WinnerAndDifference);

            var realLoser = realOutcome > 0 ? realAway : realHome;
            var betLoser = betOutcome > 0 ? betAway : betHome;
            if (realLoser == betLoser)
                return (rules.GetValueOrDefault(ScoreType.WinnerAndLoserGoals), ScoreType.WinnerAndLoserGoals);

            return (rules.GetValueOrDefault(ScoreType.WinnerOnly), ScoreType.WinnerOnly);
        }

        return (rules.GetValueOrDefault(ScoreType.Consolation), ScoreType.Consolation);
    }
}
