using FluentValidation;
using Headsoft.Core;
using Headsoft.Core.Extensions;
using Headsoft.Core.Interfaces.Data;
using Headsoft.Messaging.Abstractions;
using Headsoft.Messaging.Abstractions.Commands;
using HeadBet.Core.Domain.Entities;
using HeadBet.Core.Domain.Enums;
using HeadBet.Core.Domain.Interfaces;
using HeadBet.Core.Domain.Interfaces.Repositories;

namespace HeadBet.Core.Application.Commands;

// --- Command ---
public class JoinPoolCommand : CommandBase<OperationResult<JoinPoolResult>>
{
    public Guid PoolId { get; set; }
    public JoinSource Source { get; set; }
}

public class JoinPoolResult
{
    public Guid PoolId { get; set; }
    public PoolMemberStatus Status { get; set; }
}

// --- Validator ---
public sealed class JoinPoolCommandValidator : AbstractValidator<JoinPoolCommand>
{
    public JoinPoolCommandValidator()
    {
        RuleFor(x => x.PoolId)
            .NotEmpty()
                .WithNotification(GenericMessages.FIELD_REQUIRED, "Bolão é obrigatório.", nameof(JoinPoolCommand.PoolId));
    }
}

// --- Handler ---
public sealed class JoinPoolCommandHandler(
    IPoolRepository poolRepository,
    IPoolMemberRepository memberRepository,
    IUnitOfWork uow,
    IUserContext userContext) : ICommandHandler<JoinPoolCommand, OperationResult<JoinPoolResult>>
{
    public async Task<OperationResult<JoinPoolResult>> HandleAsync(JoinPoolCommand command, CancellationToken ct)
    {
        var userId = userContext.UserId;

        var pool = await poolRepository.GetAsync(p => p.Id == command.PoolId, @readonly: true, ct);
        if (pool is null)
            return Result.Warning<JoinPoolResult>(GenericMessages.ITEM_NOT_FOUND, "Bolão não encontrado.");

        if (!pool.IsActive)
            return Result.Warning<JoinPoolResult>(GenericMessages.INVALID_OPERATION, "Este bolão não está ativo.");

        // "Entrar" so funciona em bolao publico. Convite por link funciona em qualquer bolao.
        if (command.Source == JoinSource.EnterButton && !pool.IsPublic)
            return Result.Warning<JoinPoolResult>(GenericMessages.INVALID_OPERATION, "Este bolão é privado. Para entrar, é necessário um link de convite.");

        var existing = await memberRepository.GetAsync(
            m => m.PoolId == command.PoolId && m.UserId == userId,
            @readonly: false,
            ct);

        if (existing is not null)
        {
            // Ja e membro ativo -> no-op, so avisa
            if (existing.Status == PoolMemberStatus.Active)
                return Result.Success(new JoinPoolResult { PoolId = pool.Id, Status = PoolMemberStatus.Active });

            // Link de convite "promove" membro pendente/inativo a Active
            if (command.Source == JoinSource.InviteLink)
            {
                existing.Status = PoolMemberStatus.Active;
                existing.JoinedAt = DateTime.UtcNow;

                var update = await uow.SaveChangesAsync(ct);
                return update.IsValid
                    ? Result.Success(new JoinPoolResult { PoolId = pool.Id, Status = PoolMemberStatus.Active })
                    : update.Cast<JoinPoolResult>();
            }

            // Via botao Entrar com status Pending/Inactive -> mantem o estado
            return Result.Warning<JoinPoolResult>(GenericMessages.INVALID_OPERATION,
                existing.Status == PoolMemberStatus.Pending
                    ? "Sua solicitação de entrada já está aguardando aprovação."
                    : "Seu vínculo com este bolão está inativo. Solicite ao administrador um link de convite.");
        }

        var status = command.Source == JoinSource.InviteLink
            ? PoolMemberStatus.Active
            : (pool.AutoAccept ? PoolMemberStatus.Active : PoolMemberStatus.Pending);

        await memberRepository.AddAsync(new PoolMember
        {
            PoolId = pool.Id,
            UserId = userId,
            Role = PoolMemberRole.Participant,
            Status = status,
            JoinedAt = DateTime.UtcNow,
        }, ct);

        var result = await uow.SaveChangesAsync(ct);

        return result.IsValid
            ? Result.Success(new JoinPoolResult { PoolId = pool.Id, Status = status })
            : result.Cast<JoinPoolResult>();
    }
}
