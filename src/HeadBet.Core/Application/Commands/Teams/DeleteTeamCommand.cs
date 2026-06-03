using Headsoft.Core;
using Headsoft.Core.Interfaces.Data;
using Headsoft.Messaging.Abstractions;
using Headsoft.Messaging.Abstractions.Commands;
using HeadBet.Core.Domain.Enums;
using HeadBet.Core.Domain.Interfaces;
using HeadBet.Core.Domain.Interfaces.Repositories;

namespace HeadBet.Core.Application.Commands;

// --- Command ---
public class DeleteTeamCommand : CommandBase<OperationResult>
{
    public Guid Id { get; set; }
    public Guid PoolId { get; set; }
}

// --- Handler ---
public sealed class DeleteTeamCommandHandler(
    ITeamRepository teamRepository,
    IPoolMemberRepository memberRepository,
    IMatchRepository matchRepository,
    IUnitOfWork uow,
    IUserContext userContext) : ICommandHandler<DeleteTeamCommand, OperationResult>
{
    public async Task<OperationResult> HandleAsync(DeleteTeamCommand command, CancellationToken ct)
    {
        var isAdmin = await memberRepository.AnyAsync(
            m => m.PoolId == command.PoolId
                 && m.UserId == userContext.UserId
                 && m.Role == PoolMemberRole.Admin
                 && m.Status == PoolMemberStatus.Active,
            ct);

        if (!isAdmin)
            return Result.Warning(GenericMessages.INVALID_OPERATION, "Apenas o administrador pode apagar times.");

        var hasMatches = await matchRepository.AnyAsync(
            m => m.PoolId == command.PoolId && (m.HomeTeamId == command.Id || m.AwayTeamId == command.Id), ct);
        if (hasMatches)
            return Result.Warning(GenericMessages.INVALID_OPERATION, "Time já possui jogos cadastrados e não pode ser apagado.");

        await teamRepository.DeleteAsync([command.Id], ct);
        return await uow.SaveChangesAsync(ct);
    }
}
