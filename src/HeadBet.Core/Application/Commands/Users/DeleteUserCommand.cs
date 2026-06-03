using Headsoft.Core;
using Headsoft.Core.Interfaces.Data;
using Headsoft.Messaging.Abstractions;
using Headsoft.Messaging.Abstractions.Commands;
using HeadBet.Core.Domain.Interfaces.Repositories;

namespace HeadBet.Core.Application.Commands;

// --- Command ---
public class DeleteUserCommand : CommandBase<OperationResult>
{
    public Guid Id { get; set; }
}

// --- Handler ---
public sealed class DeleteUserCommandHandler(
    IUserRepository repository,
    IUnitOfWork uow) : ICommandHandler<DeleteUserCommand, OperationResult>
{
    public async Task<OperationResult> HandleAsync(DeleteUserCommand command, CancellationToken ct)
    {
        await repository.DeleteAsync([command.Id], ct);
        return await uow.SaveChangesAsync(ct);
    }
}
