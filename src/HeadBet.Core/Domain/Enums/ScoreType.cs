using System.ComponentModel.DataAnnotations;

namespace HeadBet.Core.Domain.Enums;

public enum ScoreType
{
    [Display(Name = "Placar exato")]
    ExactScore = 1,

    // Valor 7 (e não 2) para não renumerar valores já persistidos em PoolScoringRule/MatchUserScore.
    [Display(Name = "Vencedor + gols do vencedor")]
    WinnerAndWinnerGoals = 7,

    [Display(Name = "Vencedor + diferença de gols / Empate com placar diferente")]
    WinnerAndDifference = 2,

    [Display(Name = "Vencedor + gols do perdedor")]
    WinnerAndLoserGoals = 3,

    [Display(Name = "Apenas o vencedor")]
    WinnerOnly = 4,

    [Display(Name = "Consolação")]
    Consolation = 5,

    [Display(Name = "Não palpitou")]
    NoBet = 6
}
