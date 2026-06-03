using FluentValidation;
using Headsoft.Core;
using Headsoft.Core.Extensions;
using Headsoft.Core.Helpers;
using Headsoft.Core.Interfaces.Data;
using Headsoft.Messaging.Abstractions;
using Headsoft.Messaging.Abstractions.Commands;
using HeadBet.Core.Application.DTOs;
using HeadBet.Core.Domain;
using HeadBet.Core.Domain.Entities;
using HeadBet.Core.Domain.Interfaces;
using HeadBet.Core.Domain.Interfaces.Repositories;

namespace HeadBet.Core.Application.Commands;

// --- Command ---
public class RegisterUserCommand : CommandBase<OperationResult<UserSessionDTO>>
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

// --- Validator ---
public sealed class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator(IUserRepository repository)
    {
        RuleFor(x => x.Name)
            .NotEmpty()
                .WithNotification(GenericMessages.FIELD_REQUIRED, "Nome é obrigatório.", nameof(RegisterUserCommand.Name))
            .MaximumLength(100)
                .WithNotification(GenericMessages.FIELD_LENGTH, "Nome deve ter no máximo 100 caracteres.", nameof(RegisterUserCommand.Name));

        RuleFor(x => x.Email)
            .NotEmpty()
                .WithNotification(GenericMessages.FIELD_REQUIRED, "E-mail é obrigatório.", nameof(RegisterUserCommand.Email))
            .EmailAddress()
                .WithNotification(GenericMessages.FIELD_FORMAT, "E-mail em formato inválido.", nameof(RegisterUserCommand.Email))
            .MaximumLength(200)
                .WithNotification(GenericMessages.FIELD_LENGTH, "E-mail deve ter no máximo 200 caracteres.", nameof(RegisterUserCommand.Email))
            .MustAsync(async (email, ct) => !await repository.EmailExistsAsync(email, excludeId: null, ct))
                .WithNotification(GenericMessages.FIELD_UNIQUE, "Já existe um usuário com este e-mail.", nameof(RegisterUserCommand.Email));

        RuleFor(x => x.Password)
            .NotEmpty()
                .WithNotification(GenericMessages.FIELD_REQUIRED, "Senha é obrigatória.", nameof(RegisterUserCommand.Password))
            .MinimumLength(6)
                .WithNotification(GenericMessages.FIELD_LENGTH, "Senha deve ter no mínimo 6 caracteres.", nameof(RegisterUserCommand.Password))
            .MaximumLength(100)
                .WithNotification(GenericMessages.FIELD_LENGTH, "Senha deve ter no máximo 100 caracteres.", nameof(RegisterUserCommand.Password));
    }
}

// --- Handler ---
public sealed class RegisterUserCommandHandler(
    IUserRepository repository,
    IUnitOfWork uow,
    IPasswordHasher hasher) : ICommandHandler<RegisterUserCommand, OperationResult<UserSessionDTO>>
{
    public async Task<OperationResult<UserSessionDTO>> HandleAsync(RegisterUserCommand command, CancellationToken ct)
    {
        var entity = new User
        {
            Id = UIDGen.NewGuid(),
            Name = command.Name,
            Email = command.Email,
            PasswordHash = hasher.Hash(command.Password),
            Role = Roles.USER
        };

        await repository.AddAsync(entity, ct);
        var result = await uow.SaveChangesAsync(ct);

        if (!result.IsValid)
            return result.Cast<UserSessionDTO>();

        return Result.Success(new UserSessionDTO(entity.Id, entity.Name, entity.Email, entity.Role));
    }
}
