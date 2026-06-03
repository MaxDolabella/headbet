using System.ComponentModel.DataAnnotations;

namespace HeadBet.Core.Models;

/// <summary>
/// ViewModel da tela self-service "Minha conta". Email é apenas leitura
/// (identidade de login); nome, telefone e opt-in de WhatsApp são editáveis.
/// </summary>
public class ProfileViewModel
{
    public Guid Id { get; set; }

    [Required(ErrorMessage = "Nome é obrigatório.")]
    [StringLength(100, ErrorMessage = "Nome deve ter no máximo 100 caracteres.")]
    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    [StringLength(20, ErrorMessage = "Telefone deve ter no máximo 20 caracteres.")]
    public string? PhoneNumber { get; set; }

    public bool WhatsAppOptIn { get; set; }
}
