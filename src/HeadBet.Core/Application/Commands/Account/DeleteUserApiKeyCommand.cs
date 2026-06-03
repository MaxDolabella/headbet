using Headsoft.Core;
using Headsoft.Core.Interfaces.Data;
using Headsoft.Messaging.Abstractions;
using Headsoft.Messaging.Abstractions.Commands;
using HeadBet.Core.Domain.Interfaces;
using HeadBet.Core.Domain.Interfaces.Repositories;

namespace HeadBet.Core.Application.Commands;

// --- Command ---
public class DeleteUserApiKeyCommand : CommandBase<OperationResult>
{
    public Guid Id { get; set; }
}

// --- Handler ---
public sealed class DeleteUserApiKeyCommandHandler(
    IUserApiKeyRepository repository,
    IUnitOfWork uow,
    IUserContext userContext) : ICommandHandler<DeleteUserApiKeyCommand, OperationResult>
{
    public async Task<OperationResult> HandleAsync(DeleteUserApiKeyCommand command, CancellationToken ct)
    {
        var entity = await repository.GetAsync(
            k => k.Id == command.Id && k.UserId == userContext.UserId, @readonly: true, ct);

        if (entity is null)
            return Result.Warning(GenericMessages.ITEM_NOT_FOUND, "API Key não encontrada.");

        await repository.DeleteAsync([command.Id], ct);
        return await uow.SaveChangesAsync(ct);
    }
}
