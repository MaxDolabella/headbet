using HeadBet.Core.Domain.Enums;

namespace HeadBet.Core.Models;

public class BetListItemViewModel
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public int? HomeScore { get; set; }
    public int? AwayScore { get; set; }
    public int Points { get; set; }
    public ScoreType? AppliedRule { get; set; }
    public bool CanShowBet { get; set; }
    public bool IsMe { get; set; }

    public bool HasBet => HomeScore.HasValue && AwayScore.HasValue;
}
