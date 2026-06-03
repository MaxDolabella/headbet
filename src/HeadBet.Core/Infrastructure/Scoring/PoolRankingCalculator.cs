using HeadBet.Core.Domain.Entities;
using HeadBet.Core.Domain.Enums;
using HeadBet.Core.Models;

namespace HeadBet.Core.Infrastructure.Scoring;

public static class PoolRankingCalculator
{
    private static readonly ScoreType[] TIEBREAKER_ORDER =
    [
        ScoreType.ExactScore,
        ScoreType.WinnerAndWinnerGoals,
        ScoreType.WinnerAndDifference,
        ScoreType.WinnerAndLoserGoals,
        ScoreType.WinnerOnly,
        ScoreType.Consolation,
    ];

    public static List<RankingItemViewModel> Compute(
        IReadOnlyList<(Guid UserId, string UserName)> members,
        IReadOnlyList<MatchUserScore> scores,
        IReadOnlyList<PoolPrize> prizes,
        PrizeMode prizeMode,
        decimal? entryFee,
        Guid? currentUserId)
    {
        var scoresByUser = scores
            .GroupBy(s => s.UserId)
            .ToDictionary(g => g.Key, g => g.ToList());

        var items = members
            .Select(m =>
            {
                scoresByUser.TryGetValue(m.UserId, out var userScores);
                userScores ??= [];
                var counts = TIEBREAKER_ORDER.ToDictionary(
                    st => st,
                    st => userScores.Count(s => s.AppliedRule == st));
                return new RankingItemViewModel
                {
                    UserId = m.UserId,
                    UserName = m.UserName,
                    TotalPoints = userScores.Sum(s => s.Points),
                    CountsByScoreType = counts,
                    IsCurrentUser = currentUserId.HasValue && m.UserId == currentUserId.Value,
                };
            })
            .ToList();

        items.Sort(Compare);
        AssignPositions(items);
        DistributePrizes(items, prizes, prizeMode, entryFee, members.Count);

        return items;
    }

    private static int Compare(RankingItemViewModel a, RankingItemViewModel b)
    {
        var byPoints = b.TotalPoints.CompareTo(a.TotalPoints);
        if (byPoints != 0) return byPoints;

        foreach (var st in TIEBREAKER_ORDER)
        {
            var byCount = b.CountsByScoreType[st].CompareTo(a.CountsByScoreType[st]);
            if (byCount != 0) return byCount;
        }

        return string.Compare(a.UserName, b.UserName, StringComparison.CurrentCultureIgnoreCase);
    }

    private static bool AreTied(RankingItemViewModel a, RankingItemViewModel b)
    {
        if (a.TotalPoints != b.TotalPoints) return false;
        foreach (var st in TIEBREAKER_ORDER)
            if (a.CountsByScoreType[st] != b.CountsByScoreType[st]) return false;
        return true;
    }

    private static void AssignPositions(List<RankingItemViewModel> items)
    {
        for (var i = 0; i < items.Count; i++)
        {
            items[i].Position = (i > 0 && AreTied(items[i], items[i - 1]))
                ? items[i - 1].Position
                : i + 1;
        }
    }

    private static void DistributePrizes(
        List<RankingItemViewModel> items,
        IReadOnlyList<PoolPrize> prizes,
        PrizeMode prizeMode,
        decimal? entryFee,
        int activeMemberCount)
    {
        if (prizes.Count == 0) return;

        var prizeBySlot = prizes.ToDictionary(
            p => p.Position,
            p => CalculateSlotAmount(p, prizeMode, entryFee, activeMemberCount));

        var groups = items
            .GroupBy(x => x.Position)
            .OrderBy(g => g.Key)
            .ToList();

        foreach (var group in groups)
        {
            var startSlot = group.Key;
            var groupSize = group.Count();
            decimal totalForGroup = 0m;
            var hasAnyAwardSlot = false;
            for (var slot = startSlot; slot < startSlot + groupSize; slot++)
            {
                if (prizeBySlot.TryGetValue(slot, out var amount))
                {
                    totalForGroup += amount;
                    hasAnyAwardSlot = true;
                }
            }

            if (!hasAnyAwardSlot) continue;

            var perPerson = totalForGroup / groupSize;
            foreach (var item in group)
            {
                item.PrizeAmount = perPerson;
                item.IsAwarded = perPerson > 0;
            }
        }
    }

    private static decimal CalculateSlotAmount(
        PoolPrize prize, PrizeMode prizeMode, decimal? entryFee, int activeMemberCount)
    {
        return prizeMode == PrizeMode.Fixed
            ? prize.FixedAmount ?? 0m
            : (entryFee ?? 0m) * activeMemberCount * (prize.Percentage ?? 0m) / 100m;
    }
}
