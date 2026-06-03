using System.ComponentModel.DataAnnotations;

namespace HeadBet.Core.Domain.Enums;

public enum PoolMemberStatus
{
    [Display(Name = "Pendente")]
    Pending = 1,

    [Display(Name = "Ativo")]
    Active = 2,

    [Display(Name = "Inativo")]
    Inactive = 3
}
