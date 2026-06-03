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
public class UpdateUserCommand : CommandBase<OperationResult>
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    /// <summary>Opcional. Quando nulo ou vazio, a senha não é alterada.</summary>
    public string? Password { get; set; }
}

// --- Validator ---
public sealed class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator(IUserRepository repository)
    {
        RuleFor(x => x.Id)
            .NotEmpty()
                .WithNotification(GenericMessages.FIELD_REQUIRED, "Id é obrigatório.", nameof(UpdateUserCommand.Id));

        RuleFor(x => x.Name)
            .NotEmpty()
                .WithNotification(GenericMessages.FIELD_REQUIRED, "Nome é obrigatório.", nameof(UpdateUserCommand.Name))
            .MaximumLength(100)
                .WithNotification(GenericMessages.FIELD_LENGTH, "Nome deve ter no máximo 100 caracteres.", nameof(UpdateUserCommand.Name));

        RuleFor(x => x.Email)
            .NotEmpty()
                .WithNotification(GenericMessages.FIELD_REQUIRED, "E-mail é obrigatório.", nameof(UpdateUserCommand.Email))
            .EmailAddress()
                .WithNotification(GenericMessages.FIELD_FORMAT, "E-mail em formato inválido.", nameof(UpdateUserCommand.Email))
            .MaximumLength(200)
                .WithNotification(GenericMessages.FIELD_LENGTH, "E-mail deve ter no máximo 200 caracteres.", nameof(UpdateUserCommand.Email));

        RuleFor(x => x)
            .MustAsync(async (cmd, ct) => !await repository.EmailExistsAsync(cmd.Email, excludeId: cmd.Id, ct))
                .WithNotification(GenericMessages.FIELD_UNIQUE, "Já existe outro usuário com este e-mail.", nameof(UpdateUserCommand.Email))
            .When(x => !string.IsNullOrWhiteSpace(x.Email));

        When(x => !string.IsNullOrWhiteSpace(x.Password), () =>
        {
            RuleFor(x => x.Password!)
                .MinimumLength(6)
                    .WithNotification(GenericMessages.FIELD_LENGTH, "Senha deve ter no mínimo 6 caracteres.", nameof(UpdateUserCommand.Password))
                .MaximumLength(100)
                    .WithNotification(GenericMessages.FIELD_LENGTH, "Senha deve ter no máximo 100 caracteres.", nameof(UpdateUserCommand.Password));
        });
    }
}

// --- Handler ---
public sealed class UpdateUserCommandHandler(
    IUserRepository repository,
    IUnitOfWork uow,
    IPasswordHasher hasher) : ICommandHandler<UpdateUserCommand, OperationResult>
{
    public async Task<OperationResult> HandleAsync(UpdateUserCommand command, CancellationToken ct)
    {
        var entity = await repository.GetAsync(e => e.Id == command.Id, @readonly: false, ct);
        if (entity is null)
            return Result.Warning(GenericMessages.ITEM_NOT_FOUND, "Usuário não encontrado.");

        entity.Name = command.Name;
        entity.Email = command.Email;

        if (!string.IsNullOrWhiteSpace(command.Password))
            entity.PasswordHash = hasher.Hash(command.Password);

        return await uow.SaveChangesAsync(ct);
    }
}
