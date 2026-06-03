using System.ComponentModel.DataAnnotations;

namespace HeadBet.Core.Models;

public class UserApiKeyFormViewModel
{
    public Guid Id { get; set; }

    [Required(ErrorMessage = "Modelo de IA é obrigatório.")]
    public Guid AiModelId { get; set; }

    [Required(ErrorMessage = "API Key é obrigatória.")]
    [StringLength(500, ErrorMessage = "API Key deve ter no máximo 500 caracteres.")]
    public string ApiKey { get; set; } = string.Empty;

    public bool IsDefault { get; set; }
}
