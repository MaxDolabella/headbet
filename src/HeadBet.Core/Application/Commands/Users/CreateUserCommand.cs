using AutoMapper;
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
public class CreateUserCommand : CommandBase<OperationResult<Guid>>
{
    public Guid Id { get; set; } = UIDGen.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

// --- Validator ---
public sealed class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator(IUserRepository repository)
    {
        RuleFor(x => x.Name)
            .NotEmpty()
                .WithNotification(GenericMessages.FIELD_REQUIRED, "Nome é obrigatório.", nameof(CreateUserCommand.Name))
            .MaximumLength(100)
                .WithNotification(GenericMessages.FIELD_LENGTH, "Nome deve ter no máximo 100 caracteres.", nameof(CreateUserCommand.Name));

        RuleFor(x => x.Email)
            .NotEmpty()
                .WithNotification(GenericMessages.FIELD_REQUIRED, "E-mail é obrigatório.", nameof(CreateUserCommand.Email))
            .EmailAddress()
                .WithNotification(GenericMessages.FIELD_FORMAT, "E-mail em formato inválido.", nameof(CreateUserCommand.Email))
            .MaximumLength(200)
                .WithNotification(GenericMessages.FIELD_LENGTH, "E-mail deve ter no máximo 200 caracteres.", nameof(CreateUserCommand.Email))
            .MustAsync(async (email, ct) => !await repository.EmailExistsAsync(email, excludeId: null, ct))
                .WithNotification(GenericMessages.FIELD_UNIQUE, "Já existe um usuário com este e-mail.", nameof(CreateUserCommand.Email));

        RuleFor(x => x.Password)
            .NotEmpty()
                .WithNotification(GenericMessages.FIELD_REQUIRED, "Senha é obrigatória na criação do usuário.", nameof(CreateUserCommand.Password))
            .MinimumLength(6)
                .WithNotification(GenericMessages.FIELD_LENGTH, "Senha deve ter no mínimo 6 caracteres.", nameof(CreateUserCommand.Password))
            .MaximumLength(100)
                .WithNotification(GenericMessages.FIELD_LENGTH, "Senha deve ter no máximo 100 caracteres.", nameof(CreateUserCommand.Password));
    }
}

// --- Handler ---
public sealed class CreateUserCommandHandler(
    IUserRepository repository,
    IUnitOfWork uow,
    IPasswordHasher hasher,
    IMapper mapper) : ICommandHandler<CreateUserCommand, OperationResult<Guid>>
{
    public async Task<OperationResult<Guid>> HandleAsync(CreateUserCommand command, CancellationToken ct)
    {
        var entity = mapper.Map<User>(command);
        entity.PasswordHash = hasher.Hash(command.Password);

        await repository.AddAsync(entity, ct);
        var result = await uow.SaveChangesAsync(ct);

        return result.IsValid
            ? Result.Success(entity.Id)
            : result.Cast<Guid>();
    }
}
