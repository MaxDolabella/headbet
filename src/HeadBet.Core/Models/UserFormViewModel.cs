using System.ComponentModel.DataAnnotations;

namespace HeadBet.Core.Models;

public class UserFormViewModel
{
    public Guid Id { get; set; }

    [Required(ErrorMessage = "Nome é obrigatório.")]
    [StringLength(100, ErrorMessage = "Nome deve ter no máximo 100 caracteres.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "E-mail é obrigatório.")]
    [EmailAddress(ErrorMessage = "E-mail inválido.")]
    [StringLength(200, ErrorMessage = "E-mail deve ter no máximo 200 caracteres.")]
    public string Email { get; set; } = string.Empty;

    // Password opcional em edição (vazio = não alterar). Em Create, obrigatório via validação no handler.
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Senha deve ter entre 6 e 100 caracteres.")]
    [DataType(DataType.Password)]
    public string? Password { get; set; }
}
