namespace HeadBet.Core.Domain.Interfaces;

/// <summary>
/// Representa o usuário da requisição atual. Implementação em Infrastructure
/// lê os dados do ClaimsPrincipal (cookie de autenticação).
/// </summary>
public interface IUserContext
{
    bool IsAuthenticated { get; }
    Guid UserId { get; }
    string Name { get; }
    string Email { get; }
    string Role { get; }
    bool IsAdmin { get; }

    /// <summary>
    /// Retorna o <see cref="UserId"/> do usuário autenticado. Lança
    /// <see cref="InvalidOperationException"/> quando não há usuário
    /// autenticado — use em handlers que exigem identidade resolvida.
    /// </summary>
    Guid RequireUserId();
}
