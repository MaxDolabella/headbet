using System.Security.Cryptography;
using FluentValidation;
using Headsoft.Core;
using Headsoft.Core.Extensions;
using Headsoft.Core.Helpers;
using Headsoft.Core.Interfaces.Data;
using Headsoft.Messaging.Abstractions;
using Headsoft.Messaging.Abstractions.Commands;
using HeadBet.Core.Domain.Entities;
using HeadBet.Core.Domain.Interfaces;
using HeadBet.Core.Domain.Interfaces.Repositories;

namespace HeadBet.Core.Application.Commands;

// --- Command ---
// Sempre retorna sucesso (mesmo quando o e-mail não existe) para não revelar quais
// e-mails estão cadastrados. ResetUrlTemplate deve conter o placeholder TOKEN_PLACEHOLDER,
// que o handler substitui pelo token gerado ao montar o link do e-mail.
public class ForgotPasswordCommand : CommandBase<OperationResult>
{
    public const string TOKEN_PLACEHOLDER = "__RESET_TOKEN__";

    public string Email { get; set; } = string.Empty;
    public string ResetUrlTemplate { get; set; } = string.Empty;
}

// --- Validator ---
public sealed class ForgotPasswordCommandValidator : AbstractValidator<ForgotPasswordCommand>
{
    public ForgotPasswordCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
                .WithNotification(GenericMessages.FIELD_REQUIRED, "E-mail é obrigatório.", nameof(ForgotPasswordCommand.Email))
            .EmailAddress()
                .WithNotification(GenericMessages.FIELD_FORMAT, "E-mail em formato inválido.", nameof(ForgotPasswordCommand.Email));
    }
}

// --- Handler ---
public sealed class ForgotPasswordCommandHandler(
    IUserRepository userRepository,
    IPasswordResetTokenRepository tokenRepository,
    IUnitOfWork uow,
    IEmailQueue emailQueue) : ICommandHandler<ForgotPasswordCommand, OperationResult>
{
    private const int TOKEN_BYTES = 32;
    private const int TOKEN_LIFETIME_HOURS = 2;
    private const string RESET_EMAIL_SUBJECT = "HeadBet — Redefinição de senha";

    public async Task<OperationResult> HandleAsync(ForgotPasswordCommand command, CancellationToken ct)
    {
        var user = await userRepository.GetByEmailAsync(command.Email, @readonly: true, ct);

        // Não revela se o e-mail existe — apenas não envia nada.
        if (user is null)
            return Result.Success();

        var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(TOKEN_BYTES));

        await tokenRepository.AddAsync(new PasswordResetToken
        {
            Id = UIDGen.NewGuid(),
            UserId = user.Id,
            Token = token,
            ExpiresAtUtc = DateTime.UtcNow.AddHours(TOKEN_LIFETIME_HOURS),
            CreatedAt = DateTime.UtcNow,
        }, ct);

        var saveResult = await uow.SaveChangesAsync(ct);
        if (!saveResult.IsValid)
            return saveResult;

        var link = command.ResetUrlTemplate.Replace(ForgotPasswordCommand.TOKEN_PLACEHOLDER, token);
        var body = BuildBody(user.Name, link);

        // Enfileira e volta na hora. O envio (com concorrência limitada, retry e timeout)
        // roda no EmailBackgroundService. Fila cheia → descartado e logado lá; a request
        // não é afetada e nunca revela se o e-mail existe.
        emailQueue.Enqueue(new EmailJob(user.Email, RESET_EMAIL_SUBJECT, body));

        return Result.Success();
    }

    private static string BuildBody(string name, string link) =>
        $"""
        <p>Olá, {System.Net.WebUtility.HtmlEncode(name)}!</p>
        <p>Recebemos um pedido para redefinir a senha da sua conta no HeadBet.</p>
        <p><a href="{link}">Clique aqui para criar uma nova senha</a>.</p>
        <p>Se o botão não funcionar, copie e cole este endereço no navegador:<br>{link}</p>
        <p>Este link expira em {TOKEN_LIFETIME_HOURS} horas. Se você não fez esse pedido, ignore este e-mail.</p>
        """;
}
