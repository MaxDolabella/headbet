using System.ComponentModel.DataAnnotations;

namespace HeadBet.Core.Domain.Enums;

public enum AiProvider
{
    [Display(Name = "Claude")]
    Claude = 1,

    [Display(Name = "OpenAI")]
    OpenAI = 2
}
