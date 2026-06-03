using System.Security.Claims;
using HeadBet.Core.Domain;
using HeadBet.Core.Domain.Interfaces;
using Microsoft.AspNetCore.Components.Authorization;

namespace HeadBet.Blazor.Infrastructure.Identity;

/// <summary>
/// IUserContext para Blazor Web App.
///
/// Prioriza o <see cref="AuthenticationStateProvider"/> (válido durante todo o
/// circuito SignalR em Interactive Server) e cai pro <see cref="IHttpContextAccessor"/>
/// apenas no render SSR/estático, onde o HttpContext existe.
/// </summary>
public sealed class BlazorUserContext(
    AuthenticationStateProvider authStateProvider,
    IHttpContextAccessor accessor) : IUserContext
{
    private ClaimsPrincipal? Principal
    {
        get
        {
            // Em Blazor Server o AuthenticationStateProvider mantém um Task
            // já completado após o circuit start, então .GetAwaiter().GetResult()
            // não bloqueia thread — só desempacota o ClaimsPrincipal cacheado.
            var principalFromCircuit = authStateProvider
                .GetAuthenticationStateAsync()
                .GetAwaiter()
                .GetResult()
                .User;

            if (principalFromCircuit.Identity?.IsAuthenticated == true)
                return principalFromCircuit;

            return accessor.HttpContext?.User;
        }
    }

    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated ?? false;

    public Guid UserId
    {
        get
        {
            var raw = Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(raw, out var id) ? id : Guid.Empty;
        }
    }

    public string Name => Principal?.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty;
    public string Email => Principal?.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;
    public string Role => Principal?.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
    public bool IsAdmin => Principal?.IsInRole(Roles.ADMIN) ?? false;

    public Guid RequireUserId()
    {
        if (!IsAuthenticated || UserId == Guid.Empty)
            throw new InvalidOperationException("Usuário não autenticado.");

        return UserId;
    }
}
