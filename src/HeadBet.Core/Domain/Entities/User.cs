using Headsoft.Core.Entities;

namespace HeadBet.Core.Domain.Entities;

// Role default Roles.USER — admins sao criados via seed de startup (ver Program.cs).

public class User : Entity<Guid>
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = Roles.USER;

    /// <summary>Telefone (E.164, ex.: +5531...) usado para notificações via WhatsApp. Opcional.</summary>
    public string? PhoneNumber { get; set; }

    /// <summary>Consentimento explícito para receber lembretes/notificações via WhatsApp.</summary>
    public bool WhatsAppOptIn { get; set; }
}
