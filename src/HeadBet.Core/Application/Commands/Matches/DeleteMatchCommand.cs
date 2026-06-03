using Headsoft.Core;
using Headsoft.Core.Interfaces.Data;
using Headsoft.Messaging.Abstractions;
using Headsoft.Messaging.Abstractions.Commands;
using HeadBet.Core.Domain.Enums;
using HeadBet.Core.Domain.Interfaces;
using HeadBet.Core.Domain.Interfaces.Repositories;

namespace HeadBet.Core.Application.Commands;

// --- Command ---
public class DeleteMatchCommand : CommandBase<OperationResult>
{
    public Guid Id { get; set; }
    public Guid PoolId { get; set; }
}

// --- Handler ---
public sealed class DeleteMatchCommandHandler(
    IMatchRepository matchRepository,
    IPoolMemberRepository memberRepository,
    IUnitOfWork uow,
    IUserContext userContext) : ICommandHandler<DeleteMatchCommand, OperationResult>
{
    public async Task<OperationResult> HandleAsync(DeleteMatchCommand command, CancellationToken ct)
    {
        var isAdmin = await memberRepository.AnyAsync(
            m => m.PoolId == command.PoolId
                 && m.UserId == userContext.UserId
                 && m.Role == PoolMemberRole.Admin
                 && m.Status == PoolMemberStatus.Active,
            ct);

        if (!isAdmin)
            return Result.Warning(GenericMessages.INVALID_OPERATION, "Apenas o administrador pode apagar jogos.");

        var entity = await matchRepository.GetAsync(m => m.Id == command.Id && m.PoolId == command.PoolId, @readonly: true, ct);
        if (entity is null)
            return Result.Warning(GenericMessages.ITEM_NOT_FOUND, "Jogo não encontrado.");

        if (entity.Status == MatchStatus.Finished)
            return Result.Warning(GenericMessages.INVALID_OPERATION, "Jogos finalizados não podem ser apagados.");

        await matchRepository.DeleteAsync([command.Id], ct);
        return await uow.SaveChangesAsync(ct);
    }
}
