using HeadBet.Core.Domain.Enums;

namespace HeadBet.Core.Models;

public class PoolRankingViewModel
{
    public Guid PoolId { get; set; }
    public string PoolName { get; set; } = string.Empty;
    public bool IsPaid { get; set; }
    public PrizeMode PrizeMode { get; set; }
    public bool IsAnonymousView { get; set; }
    public bool IsMember { get; set; }
    public List<RankingItemViewModel> Items { get; set; } = [];

    public static readonly ScoreType[] BreakdownOrder =
    [
        ScoreType.ExactScore,
        ScoreType.WinnerAndDifference,
        ScoreType.WinnerAndLoserGoals,
        ScoreType.WinnerOnly,
        ScoreType.Consolation,
    ];
}
