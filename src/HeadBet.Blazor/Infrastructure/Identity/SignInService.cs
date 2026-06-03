using System.Security.Claims;
using Headsoft.Messaging.Abstractions;
using HeadBet.Core.Application.Commands;
using HeadBet.Core.Application.DTOs;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HeadBet.Blazor.Infrastructure.Identity;

/// <summary>
/// Encapsula a emissão/limpeza do cookie de autenticação fora do circuito Blazor.
/// Páginas estáticas (Login/Register) postam para os endpoints abaixo via &lt;form&gt;.
/// </summary>
public sealed class SignInService(IBus bus)
{
    public async Task<OperationOutcome> RegisterAsync(string name, string email, string password, CancellationToken ct)
    {
        var result = await bus.SendAsync(new RegisterUserCommand
        {
            Name = name,
            Email = email,
            Password = password,
        }, ct);

        return new OperationOutcome(result.IsValid, result.Data, result.Notifications?.Select(n => n.Details ?? n.Message).ToList() ?? []);
    }

    public async Task<OperationOutcome> LoginAsync(string email, string password, CancellationToken ct)
    {
        var result = await bus.SendAsync(new LoginUserCommand { Email = email, Password = password }, ct);
        return new OperationOutcome(result.IsValid, result.Data, result.Notifications?.Select(n => n.Details ?? n.Message).ToList() ?? []);
    }

    public static Task SignInCookieAsync(HttpContext http, UserSessionDTO session, bool persistent)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, session.Id.ToString()),
            new(ClaimTypes.Name, session.Name),
            new(ClaimTypes.Email, session.Email),
            new(ClaimTypes.Role, session.Role),
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);
        var props = new AuthenticationProperties { IsPersistent = persistent };

        return http.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, props);
    }

    // ---------- Endpoints (registrados em Program.cs) ----------

    public static async Task<IResult> SignInEndpoint(
        HttpContext http,
        [FromServices] SignInService svc,
        [FromForm] string action,
        [FromForm] string email,
        [FromForm] string password,
        [FromForm] string? name,
        [FromForm] string? confirmPassword,
        [FromForm] bool? rememberMe,
        [FromForm] string? returnUrl,
        CancellationToken ct)
    {
        if (action == "register" && password != (confirmPassword ?? string.Empty))
        {
            var mismatchQs = QueryString
                .Create("error", "As senhas não conferem.")
                .Add("email", email)
                .Add("returnUrl", returnUrl ?? string.Empty);

            return Results.Redirect("/account/register" + mismatchQs);
        }

        OperationOutcome outcome = action switch
        {
            "register" => await svc.RegisterAsync(name ?? string.Empty, email, password, ct),
            _ => await svc.LoginAsync(email, password, ct),
        };

        if (!outcome.IsValid)
        {
            var qs = QueryString
                .Create("error", string.Join(" / ", outcome.Errors))
                .Add("email", email)
                .Add("returnUrl", returnUrl ?? string.Empty);

            var redirectTo = action == "register" ? "/account/register" : "/account/login";
            return Results.Redirect(redirectTo + qs);
        }

        await SignInCookieAsync(http, (UserSessionDTO)outcome.Data!, rememberMe ?? false);

        var target = !string.IsNullOrWhiteSpace(returnUrl) && Uri.IsWellFormedUriString(returnUrl, UriKind.Relative)
            ? returnUrl
            : "/";

        return Results.Redirect(target);
    }

    public static async Task<IResult> SignOutEndpoint(HttpContext http)
    {
        await http.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Results.Redirect("/account/login");
    }
}

public sealed record OperationOutcome(bool IsValid, object? Data, List<string> Errors);
