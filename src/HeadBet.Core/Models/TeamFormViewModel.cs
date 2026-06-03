using System.ComponentModel.DataAnnotations;

namespace HeadBet.Core.Models;

public class TeamFormViewModel
{
    public Guid Id { get; set; }
    public Guid PoolId { get; set; }

    [Required(ErrorMessage = "Nome é obrigatório.")]
    [StringLength(100, ErrorMessage = "Nome deve ter no máximo 100 caracteres.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Abreviação é obrigatória.")]
    [StringLength(10, ErrorMessage = "Abreviação deve ter no máximo 10 caracteres.")]
    public string Abbreviation { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "URL deve ter no máximo 500 caracteres.")]
    public string? FlagUrl { get; set; }
}
