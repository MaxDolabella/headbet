using System.ComponentModel.DataAnnotations;

namespace HeadBet.Core.Domain.Enums;

public enum BetLogAction
{
    [Display(Name = "Criado")]
    Created = 1,

    [Display(Name = "Atualizado")]
    Updated = 2
}
