using FluentValidation;
using Headsoft.Core;
using Headsoft.Core.Extensions;
using Headsoft.Core.Interfaces.Data;
using Headsoft.Messaging.Abstractions;
using Headsoft.Messaging.Abstractions.Commands;
using HeadBet.Core.Domain.Interfaces;
using HeadBet.Core.Domain.Interfaces.Repositories;

namespace HeadBet.Core.Application.Commands;

// --- Command ---
public class ResetPasswordCommand : CommandBase<OperationResult>
{
    public string Token { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}

// --- Validator ---
public sealed class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordCommandValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty()
                .WithNotification(GenericMessages.FIELD_REQUIRED, "Token é obrigatório.", nameof(ResetPasswordCommand.Token));

        RuleFor(x => x.NewPassword)
            .NotEmpty()
                .WithNotification(GenericMessages.FIELD_REQUIRED, "Nova senha é obrigatória.", nameof(ResetPasswordCommand.NewPassword))
            .MinimumLength(6)
                .WithNotification(GenericMessages.FIELD_LENGTH, "Nova senha deve ter no mínimo 6 caracteres.", nameof(ResetPasswordCommand.NewPassword))
            .MaximumLength(100)
                .WithNotification(GenericMessages.FIELD_LENGTH, "Nova senha deve ter no máximo 100 caracteres.", nameof(ResetPasswordCommand.NewPassword));
    }
}

// --- Handler ---
public sealed class ResetPasswordCommandHandler(
    IUserRepository userRepository,
    IPasswordResetTokenRepository tokenRepository,
    IUnitOfWork uow,
    IPasswordHasher hasher) : ICommandHandler<ResetPasswordCommand, OperationResult>
{
    private const string INVALID_TOKEN_DETAILS = "Link de redefinição inválido ou expirado. Solicite um novo.";

    public async Task<OperationResult> HandleAsync(ResetPasswordCommand command, CancellationToken ct)
    {
        var token = await tokenRepository.GetByTokenAsync(command.Token, @readonly: false, ct);

        if (token is null || token.UsedAtUtc is not null || token.ExpiresAtUtc < DateTime.UtcNow)
            return Result.Error(GenericMessages.UNAUTHORIZED, INVALID_TOKEN_DETAILS);

        var user = await userRepository.GetAsync(u => u.Id == token.UserId, @readonly: false, ct);
        if (user is null)
            return Result.Error(GenericMessages.UNAUTHORIZED, INVALID_TOKEN_DETAILS);

        user.PasswordHash = hasher.Hash(command.NewPassword);
        token.UsedAtUtc = DateTime.UtcNow;

        return await uow.SaveChangesAsync(ct);
    }
}
