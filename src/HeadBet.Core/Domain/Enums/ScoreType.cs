using System.ComponentModel.DataAnnotations;

namespace HeadBet.Core.Domain.Enums;

public enum ScoreType
{
    [Display(Name = "Placar exato")]
    ExactScore = 1,

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
