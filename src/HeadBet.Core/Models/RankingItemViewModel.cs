using HeadBet.Core.Domain.Enums;

namespace HeadBet.Core.Models;

public class RankingItemViewModel
{
    public int Position { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public int TotalPoints { get; set; }
    public Dictionary<ScoreType, int> CountsByScoreType { get; set; } = new();
    public decimal PrizeAmount { get; set; }
    public bool IsAwarded { get; set; }
    public bool IsCurrentUser { get; set; }
}
