using HeadBet.Core.Domain.Enums;

namespace HeadBet.Core.Models;

public class PoolFormViewModel : IPrizesCardModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsPaid { get; set; }
    public decimal? EntryFee { get; set; }
    public bool AutoAccept { get; set; } = true;
    public bool IsActive { get; set; } = true;
    public bool IsPublic { get; set; }
    public PrizeMode PrizeMode { get; set; } = PrizeMode.Percentage;

    public List<ScoringRuleItemViewModel> ScoringRules { get; set; } = BuildDefaultScoring();
    public List<PrizeItemViewModel> Prizes { get; set; } = BuildDefaultPrizes();

    // Apenas UI — simulação de valor arrecadado para exibir valor em R$ por posição.
    public decimal SimulatedTotal { get; set; }

    private static List<ScoringRuleItemViewModel> BuildDefaultScoring() =>
    [
        new() { Type = ScoreType.ExactScore, Points = 25 },
        new() { Type = ScoreType.WinnerAndDifference, Points = 15 },
        new() { Type = ScoreType.WinnerAndLoserGoals, Points = 12 },
        new() { Type = ScoreType.WinnerOnly, Points = 10 },
        new() { Type = ScoreType.Consolation, Points = 4 },
        new() { Type = ScoreType.NoBet, Points = 0 },
    ];

    private static List<PrizeItemViewModel> BuildDefaultPrizes() =>
    [
        new() { Position = 1, Percentage = 50m },
        new() { Position = 2, Percentage = 30m },
        new() { Position = 3, Percentage = 20m },
    ];
}
