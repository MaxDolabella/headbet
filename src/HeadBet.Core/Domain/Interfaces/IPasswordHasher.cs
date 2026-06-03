namespace HeadBet.Core.Domain.Interfaces;

/// <summary>
/// Abstração de hash de senha. Implementação em Infrastructure usa PBKDF2 (SHA-256).
/// </summary>
public interface IPasswordHasher
{
    /// <summary>Gera um hash para a senha informada (com salt aleatório).</summary>
    string Hash(string password);

    /// <summary>Verifica se a senha corresponde ao hash armazenado.</summary>
    bool Verify(string password, string passwordHash);
}
