using System.Security.Claims;
using Headsoft.Core;
using Headsoft.Messaging.Abstractions;
using HeadBet.Core.Application.Commands;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HeadBet.Blazor.Infrastructure.Identity;

/// <summary>
/// Endpoints de gerenciamento de senha (fora do circuito Blazor). As páginas SSR
/// postam para cá via &lt;form&gt;; o resultado volta por redirect com ?error= / ?success=.
/// </summary>
public static class PasswordEndpoints
{
    // POST /account/change-password
    public static async Task<IResult> ChangePasswordEndpoint(
        HttpContext http,
        [FromServices] IBus bus,
        [FromForm] string currentPassword,
        [FromForm] string newPassword,
        [FromForm] string confirmPassword,
        CancellationToken ct)
    {
        var raw = http.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(raw, out var userId))
            return Results.Redirect("/account/login");

        if (newPassword != confirmPassword)
            return RedirectWithError("/account/change-password", "As senhas não conferem.");

        var result = await bus.SendAsync(new ChangePasswordCommand
        {
            UserId = userId,
            CurrentPassword = currentPassword,
            NewPassword = newPassword,
        }, ct);

        if (!result.IsValid)
            return RedirectWithError("/account/change-password", FirstError(result));

        return Results.Redirect("/" + QueryString.Create("success", "Senha alterada com sucesso."));
    }

    // POST /account/forgot-password
    public static async Task<IResult> ForgotPasswordEndpoint(
        HttpContext http,
        [FromServices] IBus bus,
        [FromForm] string email,
        CancellationToken ct)
    {
        // Link absoluto montado a partir do request; o handler troca o placeholder pelo token real.
        var resetUrlTemplate =
            $"{http.Request.Scheme}://{http.Request.Host}/account/reset-password" +
            QueryString.Create("token", ForgotPasswordCommand.TOKEN_PLACEHOLDER);

        var result = await bus.SendAsync(new ForgotPasswordCommand
        {
            Email = email,
            ResetUrlTemplate = resetUrlTemplate,
        }, ct);

        // Não revela existência do e-mail: validação só pega formato/obrigatório.
        if (!result.IsValid)
            return RedirectWithError("/account/forgot-password", FirstError(result), ("email", email));

        return Results.Redirect("/account/forgot-password-confirmation");
    }

    // POST /account/reset-password
    public static async Task<IResult> ResetPasswordEndpoint(
        HttpContext http,
        [FromServices] IBus bus,
        [FromForm] string token,
        [FromForm] string newPassword,
        [FromForm] string confirmPassword,
        CancellationToken ct)
    {
        if (newPassword != confirmPassword)
            return RedirectWithError("/account/reset-password", "As senhas não conferem.", ("token", token));

        var result = await bus.SendAsync(new ResetPasswordCommand
        {
            Token = token,
            NewPassword = newPassword,
        }, ct);

        if (!result.IsValid)
            return RedirectWithError("/account/reset-password", FirstError(result), ("token", token));

        return Results.Redirect("/account/login" + QueryString.Create("success", "Senha redefinida com sucesso. Faça login com a nova senha."));
    }

    private static IResult RedirectWithError(string path, string error, params (string Key, string Value)[] extra)
    {
        var qs = QueryString.Create("error", error);
        foreach (var (key, value) in extra)
            qs = qs.Add(key, value);

        return Results.Redirect(path + qs);
    }

    private static string FirstError(OperationResult result)
    {
        var note = result.Notifications?.FirstOrDefault();
        return note?.Details ?? note?.Message ?? "Não foi possível concluir a operação.";
    }
}
