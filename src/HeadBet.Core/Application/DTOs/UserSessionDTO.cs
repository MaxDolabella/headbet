namespace HeadBet.Core.Application.DTOs;

/// <summary>
/// Dados do usuario que vao para o ClaimsPrincipal (montado pelo AccountController
/// apos Register/Login bem sucedidos).
/// </summary>
public sealed record UserSessionDTO(Guid Id, string Name, string Email, string Role);
