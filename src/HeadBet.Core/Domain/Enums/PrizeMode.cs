using System.ComponentModel.DataAnnotations;

namespace HeadBet.Core.Domain.Enums;

public enum PrizeMode
{
    [Display(Name = "Percentual sobre o arrecadado")]
    Percentage = 1,

    [Display(Name = "Valor fixo (R$)")]
    Fixed = 2,
}
