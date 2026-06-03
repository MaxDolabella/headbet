namespace HeadBet.Core.Domain;

/// <summary>
/// Constantes de roles usadas no <c>ClaimTypes.Role</c> e em <c>[Authorize(Roles = ...)]</c>.
/// Usar string (e não enum) mantém compatibilidade com o modelo de claims do ASP.NET Core.
/// </summary>
public static class Roles
{
    public const string USER = "User";
    public const string ADMIN = "Admin";
}
