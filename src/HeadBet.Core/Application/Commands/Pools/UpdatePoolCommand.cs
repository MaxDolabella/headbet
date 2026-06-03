using AutoMapper;
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
using HeadBet.Core.Models;

namespace HeadBet.Core.Application.Commands;

// --- Command ---
public class UpdatePoolCommand : CommandBase<OperationResult>, IPoolPrizeForm
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsPaid { get; set; }
    public decimal? EntryFee { get; set; }
    public decimal? CollectedAmount { get; set; }
    public bool AutoAccept { get; set; }
    public bool IsActive { get; set; }
    public bool IsPublic { get; set; }
    public PrizeMode PrizeMode { get; set; } = PrizeMode.Percentage;
    public List<PrizeItemViewModel> Prizes { get; set; } = [];
}

// --- Validator ---
public sealed class UpdatePoolCommandValidator : AbstractValidator<UpdatePoolCommand>
{
    public UpdatePoolCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
                .WithNotification(GenericMessages.FIELD_REQUIRED, "Nome do bolão é obrigatório.", nameof(UpdatePoolCommand.Name))
            .MaximumLength(100)
                .WithNotification(GenericMessages.FIELD_LENGTH, "Nome deve ter no máximo 100 caracteres.", nameof(UpdatePoolCommand.Name));

        RuleFor(x => x.Description)
            .MaximumLength(500)
                .WithNotification(GenericMessages.FIELD_LENGTH, "Descrição deve ter no máximo 500 caracteres.", nameof(UpdatePoolCommand.Description));

        RuleFor(x => x.EntryFee)
            .GreaterThan(0)
                .When(x => x.IsPaid)
                .WithNotification(GenericMessages.FIELD_FORMAT, "Taxa de inscrição deve ser maior que zero em bolão pago.", nameof(UpdatePoolCommand.EntryFee));

        PoolPrizeValidationRules.ApplyTo(this);
    }
}

// --- Handler ---
public sealed class UpdatePoolCommandHandler(
    IPoolRepository poolRepository,
    IPoolMemberRepository memberRepository,
    IPoolPrizeRepository prizeRepository,
    IUnitOfWork uow,
    IMapper mapper,
    IUserContext userContext) : ICommandHandler<UpdatePoolCommand, OperationResult>
{
    public async Task<OperationResult> HandleAsync(UpdatePoolCommand command, CancellationToken ct)
    {
        var isAdmin = await memberRepository.AnyAsync(
            m => m.PoolId == command.Id
                 && m.UserId == userContext.UserId
                 && m.Role == PoolMemberRole.Admin
                 && m.Status == PoolMemberStatus.Active,
            ct);

        if (!isAdmin)
            return Result.Warning(GenericMessages.INVALID_OPERATION, "Apenas o administrador pode editar o bolão.");

        var entity = await poolRepository.GetAsync(p => p.Id == command.Id, @readonly: false, ct);
        if (entity is null)
            return Result.Warning(GenericMessages.ITEM_NOT_FOUND, "Bolão não encontrado.");

        mapper.Map(command, entity);

        var existingPrizes = await prizeRepository.ToListAsync(p => p.PoolId == command.Id, @readonly: false, ct);
        if (existingPrizes.Count > 0)
            prizeRepository.Delete(existingPrizes);

        if (command.IsPaid)
        {
            foreach (var prize in command.Prizes)
            {
                await prizeRepository.AddAsync(new PoolPrize
                {
                    PoolId = command.Id,
                    Position = prize.Position,
                    Percentage = command.PrizeMode == PrizeMode.Percentage ? prize.Percentage : null,
                    FixedAmount = command.PrizeMode == PrizeMode.Fixed ? prize.FixedAmount : null,
                }, ct);
            }
        }

        return await uow.SaveChangesAsync(ct);
    }
}
