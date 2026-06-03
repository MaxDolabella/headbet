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
public class ReactivatePoolMemberCommand : CommandBase<OperationResult>
{
    public Guid PoolId { get; set; }
    public Guid UserId { get; set; }
}

// --- Validator ---
public sealed class ReactivatePoolMemberCommandValidator : AbstractValidator<ReactivatePoolMemberCommand>
{
    public ReactivatePoolMemberCommandValidator()
    {
        RuleFor(x => x.PoolId)
            .NotEmpty()
                .WithNotification(GenericMessages.FIELD_REQUIRED, "Bolão é obrigatório.", nameof(ReactivatePoolMemberCommand.PoolId));
        RuleFor(x => x.UserId)
            .NotEmpty()
                .WithNotification(GenericMessages.FIELD_REQUIRED, "Participante é obrigatório.", nameof(ReactivatePoolMemberCommand.UserId));
    }
}

// --- Handler ---
public sealed class ReactivatePoolMemberCommandHandler(
    IPoolMemberRepository memberRepository,
    IUnitOfWork uow,
    IUserContext userContext) : ICommandHandler<ReactivatePoolMemberCommand, OperationResult>
{
    public async Task<OperationResult> HandleAsync(ReactivatePoolMemberCommand command, CancellationToken ct)
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
            m => m.PoolId == command.PoolId && m.UserId == command.UserId, @readonly: false, ct);

        if (member is null)
            return Result.Warning(GenericMessages.ITEM_NOT_FOUND, "Participante não encontrado neste bolão.");

        if (member.Status != PoolMemberStatus.Inactive)
            return Result.Warning(GenericMessages.INVALID_OPERATION, "Apenas participantes inativos podem ser reativados.");

        member.Status = PoolMemberStatus.Active;

        return await uow.SaveChangesAsync(ct);
    }
}
