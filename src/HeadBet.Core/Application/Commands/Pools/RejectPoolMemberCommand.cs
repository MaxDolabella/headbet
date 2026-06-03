using FluentValidation;
using Headsoft.Core;
using Headsoft.Core.Extensions;
using Headsoft.Core.Interfaces.Data;
using Headsoft.Messaging.Abstractions;
using Headsoft.Messaging.Abstractions.Commands;
using HeadBet.Core.Domain.Enums;
using HeadBet.Core.Domain.Interfaces;
using HeadBet.Core.Domain.Interfaces.Repositories;

namespace HeadBet.Core.Application.Commands;

// --- Command ---
public class RejectPoolMemberCommand : CommandBase<OperationResult>
{
    public Guid PoolId { get; set; }
    public Guid UserId { get; set; }
}

// --- Validator ---
public sealed class RejectPoolMemberCommandValidator : AbstractValidator<RejectPoolMemberCommand>
{
    public RejectPoolMemberCommandValidator()
    {
        RuleFor(x => x.PoolId)
            .NotEmpty()
                .WithNotification(GenericMessages.FIELD_REQUIRED, "Bolão é obrigatório.", nameof(RejectPoolMemberCommand.PoolId));
        RuleFor(x => x.UserId)
            .NotEmpty()
                .WithNotification(GenericMessages.FIELD_REQUIRED, "Participante é obrigatório.", nameof(RejectPoolMemberCommand.UserId));
    }
}

// --- Handler ---
public sealed class RejectPoolMemberCommandHandler(
    IPoolMemberRepository memberRepository,
    IUnitOfWork uow,
    IUserContext userContext) : ICommandHandler<RejectPoolMemberCommand, OperationResult>
{
    public async Task<OperationResult> HandleAsync(RejectPoolMemberCommand command, CancellationToken ct)
    {
        var isAdmin = await memberRepository.AnyAsync(
            m => m.PoolId == command.PoolId
                 && m.UserId == userContext.UserId
                 && m.Role == PoolMemberRole.Admin
                 && m.Status == PoolMemberStatus.Active,
            ct);

        if (!isAdmin)
            return Result.Warning(GenericMessages.INVALID_OPERATION, "Apenas o administrador pode gerenciar participantes.");

        var member = await memberRepository.GetAsync(
            m => m.PoolId == command.PoolId && m.UserId == command.UserId, @readonly: true, ct);

        if (member is null)
            return Result.Warning(GenericMessages.ITEM_NOT_FOUND, "Participante não encontrado neste bolão.");

        if (member.Status != PoolMemberStatus.Pending)
            return Result.Warning(GenericMessages.INVALID_OPERATION, "Apenas solicitações pendentes podem ser recusadas.");

        await memberRepository.DeleteAsync([member.PoolId, member.UserId], ct);

        return await uow.SaveChangesAsync(ct);
    }
}
