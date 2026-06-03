using Headsoft.Core.Entities;

namespace HeadBet.Core.Domain.Entities;

// Token de redefinição de senha enviado por e-mail. O link contém o Token (valor
// aleatório); a validação compara o valor recebido com o armazenado, exige que não
// esteja expirado (ExpiresAtUtc) nem já usado (UsedAtUtc).
public class PasswordResetToken : Entity<Guid>
{
    public Guid UserId { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAtUtc { get; set; }
    public DateTime? UsedAtUtc { get; set; }
    public DateTime CreatedAt { get; set; }

    public User User { get; set; } = null!;
}
