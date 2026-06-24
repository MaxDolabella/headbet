using Headsoft.Core;
using Headsoft.Core.Interfaces.Data;
using Headsoft.Messaging.Abstractions;
using Headsoft.Messaging.Abstractions.Commands;
using HeadBet.Core.Domain.Interfaces;
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
    IUserContext userContext,
    IUnitOfWork uow) : ICommandHandler<DeleteUserCommand, OperationResult>
{
    public async Task<OperationResult> HandleAsync(DeleteUserCommand command, CancellationToken ct)
    {
        // Defesa em profundidade: não confiar apenas no [Authorize(Roles=ADMIN)] da página.
        if (!userContext.IsAdmin)
            return Result.Warning(GenericMessages.INVALID_OPERATION, "Acesso negado.");

        await repository.DeleteAsync([command.Id], ct);
        return await uow.SaveChangesAsync(ct);
    }
}
