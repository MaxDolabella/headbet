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
public class ChangePasswordCommand : CommandBase<OperationResult>
{
    public Guid UserId { get; set; }
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}

// --- Validator ---
public sealed class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
                .WithNotification(GenericMessages.FIELD_REQUIRED, "Usuário é obrigatório.", nameof(ChangePasswordCommand.UserId));

        RuleFor(x => x.CurrentPassword)
            .NotEmpty()
                .WithNotification(GenericMessages.FIELD_REQUIRED, "Senha atual é obrigatória.", nameof(ChangePasswordCommand.CurrentPassword));

        RuleFor(x => x.NewPassword)
            .NotEmpty()
                .WithNotification(GenericMessages.FIELD_REQUIRED, "Nova senha é obrigatória.", nameof(ChangePasswordCommand.NewPassword))
            .MinimumLength(6)
                .WithNotification(GenericMessages.FIELD_LENGTH, "Nova senha deve ter no mínimo 6 caracteres.", nameof(ChangePasswordCommand.NewPassword))
            .MaximumLength(100)
                .WithNotification(GenericMessages.FIELD_LENGTH, "Nova senha deve ter no máximo 100 caracteres.", nameof(ChangePasswordCommand.NewPassword));
    }
}

// --- Handler ---
public sealed class ChangePasswordCommandHandler(
    IUserRepository repository,
    IUnitOfWork uow,
    IPasswordHasher hasher) : ICommandHandler<ChangePasswordCommand, OperationResult>
{
    private const string INVALID_CURRENT_PASSWORD_DETAILS = "Senha atual incorreta.";

    public async Task<OperationResult> HandleAsync(ChangePasswordCommand command, CancellationToken ct)
    {
        var user = await repository.GetAsync(u => u.Id == command.UserId, @readonly: false, ct);
        if (user is null)
            return Result.Warning(GenericMessages.ITEM_NOT_FOUND, "Usuário não encontrado.");

        if (string.IsNullOrEmpty(user.PasswordHash) || !hasher.Verify(command.CurrentPassword, user.PasswordHash))
            return Result.Error(GenericMessages.UNAUTHORIZED, INVALID_CURRENT_PASSWORD_DETAILS);

        user.PasswordHash = hasher.Hash(command.NewPassword);

        return await uow.SaveChangesAsync(ct);
    }
}
