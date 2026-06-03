using Headsoft.Core;
using Headsoft.Core.Interfaces.Data;
using Headsoft.Messaging.Abstractions;
using Headsoft.Messaging.Abstractions.Commands;
using HeadBet.Core.Domain.Interfaces.Repositories;

namespace HeadBet.Core.Application.Commands;

// --- Command ---
public class DeleteAiModelCommand : CommandBase<OperationResult>
{
    public Guid Id { get; set; }
}

// --- Handler ---
public sealed class DeleteAiModelCommandHandler(
    IAiModelRepository repository,
    IUnitOfWork uow) : ICommandHandler<DeleteAiModelCommand, OperationResult>
{
    public async Task<OperationResult> HandleAsync(DeleteAiModelCommand command, CancellationToken ct)
    {
        await repository.DeleteAsync([command.Id], ct);
        return await uow.SaveChangesAsync(ct);
    }
}
