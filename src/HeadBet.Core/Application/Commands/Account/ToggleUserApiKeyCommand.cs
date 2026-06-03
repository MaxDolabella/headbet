using Headsoft.Core;
using Headsoft.Core.Interfaces.Data;
using Headsoft.Messaging.Abstractions;
using Headsoft.Messaging.Abstractions.Commands;
using HeadBet.Core.Domain.Interfaces;
using HeadBet.Core.Domain.Interfaces.Repositories;

namespace HeadBet.Core.Application.Commands;

// --- Command ---
public class ToggleUserApiKeyCommand : CommandBase<OperationResult>
{
    public Guid Id { get; set; }
}

// --- Handler ---
public sealed class ToggleUserApiKeyCommandHandler(
    IUserApiKeyRepository repository,
    IUnitOfWork uow,
    IUserContext userContext) : ICommandHandler<ToggleUserApiKeyCommand, OperationResult>
{
    public async Task<OperationResult> HandleAsync(ToggleUserApiKeyCommand command, CancellationToken ct)
    {
        var entity = await repository.GetAsync(
            k => k.Id == command.Id && k.UserId == userContext.UserId, @readonly: false, ct);

        if (entity is null)
            return Result.Warning(GenericMessages.ITEM_NOT_FOUND, "API Key não encontrada.");

        if (entity.IsDefault)
        {
            entity.IsDefault = false;
        }
        else
        {
            await repository.ClearDefaultAsync(userContext.UserId, entity.Id, ct);
            entity.IsDefault = true;
        }

        return await uow.SaveChangesAsync(ct);
    }
}
