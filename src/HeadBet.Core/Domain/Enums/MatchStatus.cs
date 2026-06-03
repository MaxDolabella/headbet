using System.ComponentModel.DataAnnotations;

namespace HeadBet.Core.Domain.Enums;

public enum MatchStatus
{
    [Display(Name = "Agendado")]
    Scheduled = 1,

    [Display(Name = "Em andamento")]
    InProgress = 2,

    [Display(Name = "Finalizado")]
    Finished = 3,

    [Display(Name = "Cancelado")]
    Cancelled = 4
}
