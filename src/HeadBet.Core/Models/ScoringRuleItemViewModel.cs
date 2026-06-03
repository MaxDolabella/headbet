using HeadBet.Core.Domain.Enums;

namespace HeadBet.Core.Models;

public class ScoringRuleItemViewModel
{
    public ScoreType Type { get; set; }
    public int Points { get; set; }
}
