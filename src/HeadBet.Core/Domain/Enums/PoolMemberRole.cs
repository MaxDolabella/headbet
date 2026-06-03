using System.ComponentModel.DataAnnotations;

namespace HeadBet.Core.Domain.Enums;

public enum PoolMemberRole
{
    [Display(Name = "Administrador")]
    Admin = 1,

    [Display(Name = "Participante")]
    Participant = 2
}
