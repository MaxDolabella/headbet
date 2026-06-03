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
public class CreateUserApiKeyCommand : CommandBase<OperationResult<Guid>>
{
    public Guid Id { get; set; } = UIDGen.NewGuid();
    public Guid AiModelId { get; set; }
    public string ApiKey { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
}

// --- Validator ---
public sealed class CreateUserApiKeyCommandValidator : AbstractValidator<CreateUserApiKeyCommand>
{
    public CreateUserApiKeyCommandValidator(IUserApiKeyRepository repository, IAiModelRepository aiModelRepository, IUserContext userContext)
    {
        RuleFor(x => x.AiModelId)
            .NotEmpty()
                .WithNotification(GenericMessages.FIELD_REQUIRED, "Modelo de IA é obrigatório.", nameof(CreateUserApiKeyCommand.AiModelId));

        RuleFor(x => x)
            .MustAsync(async (cmd, ct) => await aiModelRepository.AnyAsync(m => m.Id == cmd.AiModelId && m.IsActive, ct))
                .WithNotification(GenericMessages.ITEM_NOT_FOUND, "Modelo de IA não encontrado ou inativo.", nameof(CreateUserApiKeyCommand.AiModelId))
            .When(x => x.AiModelId != Guid.Empty);

        RuleFor(x => x)
            .MustAsync(async (cmd, ct) => !await repository.AnyAsync(
                k => k.UserId == userContext.UserId && k.AiModelId == cmd.AiModelId, ct))
                .WithNotification(GenericMessages.FIELD_UNIQUE, "Já existe uma API Key cadastrada para este modelo.", nameof(CreateUserApiKeyCommand.AiModelId))
            .When(x => x.AiModelId != Guid.Empty);

        RuleFor(x => x.ApiKey)
            .NotEmpty()
                .WithNotification(GenericMessages.FIELD_REQUIRED, "API Key é obrigatória.", nameof(CreateUserApiKeyCommand.ApiKey))
            .MaximumLength(500)
                .WithNotification(GenericMessages.FIELD_LENGTH, "API Key deve ter no máximo 500 caracteres.", nameof(CreateUserApiKeyCommand.ApiKey));
    }
}

// --- Handler ---
public sealed class CreateUserApiKeyCommandHandler(
    IUserApiKeyRepository repository,
    IUnitOfWork uow,
    IUserContext userContext) : ICommandHandler<CreateUserApiKeyCommand, OperationResult<Guid>>
{
    public async Task<OperationResult<Guid>> HandleAsync(CreateUserApiKeyCommand command, CancellationToken ct)
    {
        var entity = new UserApiKey
        {
            Id = command.Id,
            UserId = userContext.UserId,
            AiModelId = command.AiModelId,
            ApiKey = command.ApiKey,
            IsDefault = command.IsDefault,
            CreatedAt = DateTime.UtcNow,
        };

        if (command.IsDefault)
            await repository.ClearDefaultAsync(userContext.UserId, command.Id, ct);

        await repository.AddAsync(entity, ct);
        var result = await uow.SaveChangesAsync(ct);

        return result.IsValid
            ? Result.Success(entity.Id)
            : result.Cast<Guid>();
    }
}
