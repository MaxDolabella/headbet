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
public class DeactivatePoolMemberCommand : CommandBase<OperationResult>
{
    public Guid PoolId { get; set; }
    public Guid UserId { get; set; }
}

// --- Validator ---
public sealed class DeactivatePoolMemberCommandValidator : AbstractValidator<DeactivatePoolMemberCommand>
{
    public DeactivatePoolMemberCommandValidator()
    {
        RuleFor(x => x.PoolId)
            .NotEmpty()
                .WithNotification(GenericMessages.FIELD_REQUIRED, "Bolão é obrigatório.", nameof(DeactivatePoolMemberCommand.PoolId));
        RuleFor(x => x.UserId)
            .NotEmpty()
                .WithNotification(GenericMessages.FIELD_REQUIRED, "Participante é obrigatório.", nameof(DeactivatePoolMemberCommand.UserId));
    }
}

// --- Handler ---
public sealed class DeactivatePoolMemberCommandHandler(
    IPoolMemberRepository memberRepository,
    IUnitOfWork uow,
    IUserContext userContext) : ICommandHandler<DeactivatePoolMemberCommand, OperationResult>
{
    public async Task<OperationResult> HandleAsync(DeactivatePoolMemberCommand command, CancellationToken ct)
    {
        var isAdmin = await memberRepository.AnyAsync(
            m => m.PoolId == command.PoolId
                 && m.UserId == userContext.UserId
                 && m.Role == PoolMemberRole.Admin
                 && m.Status == PoolMemberStatus.Active,
            ct);

        if (!isAdmin)
            return Result.Warning(GenericMessages.INVALID_OPERATION, "Apenas o administrador pode gerenciar participantes.");

        if (command.UserId == userContext.UserId)
            return Result.Warning(GenericMessages.INVALID_OPERATION, "Você não pode desativar a si mesmo.");

        var member = await memberRepository.GetAsync(
            m => m.PoolId == command.PoolId && m.UserId == command.UserId, @readonly: false, ct);

        if (member is null)
            return Result.Warning(GenericMessages.ITEM_NOT_FOUND, "Participante não encontrado neste bolão.");

        if (member.Role == PoolMemberRole.Admin)
            return Result.Warning(GenericMessages.INVALID_OPERATION, "Não é possível desativar um administrador.");

        if (member.Status != PoolMemberStatus.Active)
            return Result.Warning(GenericMessages.INVALID_OPERATION, "Apenas participantes ativos podem ser desativados.");

        member.Status = PoolMemberStatus.Inactive;

        return await uow.SaveChangesAsync(ct);
    }
}
