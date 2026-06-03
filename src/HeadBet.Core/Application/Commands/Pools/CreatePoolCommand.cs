using AutoMapper;
using FluentValidation;
using Headsoft.Core;
using Headsoft.Core.Extensions;
using Headsoft.Core.Helpers;
using Headsoft.Core.Interfaces.Data;
using Headsoft.Messaging.Abstractions;
using Headsoft.Messaging.Abstractions.Commands;
using HeadBet.Core.Domain.Entities;
using HeadBet.Core.Domain.Enums;
using HeadBet.Core.Domain.Interfaces;
using HeadBet.Core.Domain.Interfaces.Repositories;
using HeadBet.Core.Infrastructure;
using HeadBet.Core.Models;

namespace HeadBet.Core.Application.Commands;

// --- Command ---
public class CreatePoolCommand : CommandBase<OperationResult<Guid>>, IPoolPrizeForm
{
    public Guid Id { get; set; } = UIDGen.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsPaid { get; set; }
    public decimal? EntryFee { get; set; }
    public bool AutoAccept { get; set; }
    public bool IsActive { get; set; }
    public bool IsPublic { get; set; }
    public PrizeMode PrizeMode { get; set; } = PrizeMode.Percentage;
    public List<ScoringRuleItemViewModel> ScoringRules { get; set; } = [];
    public List<PrizeItemViewModel> Prizes { get; set; } = [];
}

// --- Validator ---
public sealed class CreatePoolCommandValidator : AbstractValidator<CreatePoolCommand>
{
    private const int SCORING_RULES_COUNT = 6;

    public CreatePoolCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
                .WithNotification(GenericMessages.FIELD_REQUIRED, "Nome do bolão é obrigatório.", nameof(CreatePoolCommand.Name))
            .MaximumLength(100)
                .WithNotification(GenericMessages.FIELD_LENGTH, "Nome deve ter no máximo 100 caracteres.", nameof(CreatePoolCommand.Name));

        RuleFor(x => x.Description)
            .MaximumLength(500)
                .WithNotification(GenericMessages.FIELD_LENGTH, "Descrição deve ter no máximo 500 caracteres.", nameof(CreatePoolCommand.Description));

        RuleFor(x => x.EntryFee)
            .GreaterThan(0)
                .When(x => x.IsPaid)
                .WithNotification(GenericMessages.FIELD_FORMAT, "Taxa de inscrição deve ser maior que zero em bolão pago.", nameof(CreatePoolCommand.EntryFee));

        RuleFor(x => x.ScoringRules)
            .Must(rules => rules is { Count: SCORING_RULES_COUNT })
                .WithNotification(GenericMessages.FIELD_FORMAT, "Pontuação deve conter 6 critérios.", nameof(CreatePoolCommand.ScoringRules));

        RuleForEach(x => x.ScoringRules).ChildRules(rule =>
        {
            rule.RuleFor(r => r.Points)
                .GreaterThanOrEqualTo(0)
                    .WithNotification(GenericMessages.FIELD_FORMAT, "Pontuação deve ser maior ou igual a zero.", nameof(ScoringRuleItemViewModel.Points));
        });

        PoolPrizeValidationRules.ApplyTo(this);
    }
}

// --- Handler ---
public sealed class CreatePoolCommandHandler(
    IPoolRepository poolRepository,
    IPoolScoringRuleRepository scoringRepository,
    IPoolPrizeRepository prizeRepository,
    IPoolMemberRepository memberRepository,
    IUnitOfWork uow,
    IMapper mapper,
    IUserContext userContext) : ICommandHandler<CreatePoolCommand, OperationResult<Guid>>
{
    public async Task<OperationResult<Guid>> HandleAsync(CreatePoolCommand command, CancellationToken ct)
    {
        var userId = userContext.RequireUserId();

        var pool = mapper.Map<Pool>(command);
        pool.CreatedAt = DateTime.UtcNow;
        pool.InviteCode = InviteCodeGenerator.Generate();

        await poolRepository.AddAsync(pool, ct);

        foreach (var rule in command.ScoringRules)
        {
            await scoringRepository.AddAsync(new PoolScoringRule
            {
                PoolId = pool.Id,
                ScoreType = rule.Type,
                Points = rule.Points,
            }, ct);
        }

        if (command.IsPaid)
        {
            foreach (var prize in command.Prizes)
            {
                await prizeRepository.AddAsync(new PoolPrize
                {
                    PoolId = pool.Id,
                    Position = prize.Position,
                    Percentage = command.PrizeMode == PrizeMode.Percentage ? prize.Percentage : null,
                    FixedAmount = command.PrizeMode == PrizeMode.Fixed ? prize.FixedAmount : null,
                }, ct);
            }
        }

        await memberRepository.AddAsync(new PoolMember
        {
            PoolId = pool.Id,
            UserId = userId,
            Role = PoolMemberRole.Admin,
            Status = PoolMemberStatus.Active,
            JoinedAt = DateTime.UtcNow,
        }, ct);

        var result = await uow.SaveChangesAsync(ct);

        return result.IsValid
            ? Result.Success(pool.Id)
            : result.Cast<Guid>();
    }
}
