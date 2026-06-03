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

namespace HeadBet.Core.Application.Commands;

// --- Command ---
public class SaveBetsCommand : CommandBase<OperationResult>
{
    public Guid PoolId { get; set; }
    public List<BetItemInput> Items { get; set; } = [];
}

public class BetItemInput
{
    public Guid MatchId { get; set; }
    public int? HomeScore { get; set; }
    public int? AwayScore { get; set; }
}

// --- Validator ---
public sealed class SaveBetsCommandValidator : AbstractValidator<SaveBetsCommand>
{
    public SaveBetsCommandValidator()
    {
        RuleFor(x => x.PoolId)
            .NotEmpty()
                .WithNotification(GenericMessages.FIELD_REQUIRED, "Bolão é obrigatório.", nameof(SaveBetsCommand.PoolId));

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.MatchId)
                .NotEmpty()
                    .WithNotification(GenericMessages.FIELD_REQUIRED, "Jogo é obrigatório.", nameof(BetItemInput.MatchId));

            item.RuleFor(i => i)
                .Must(i => (i.HomeScore.HasValue && i.AwayScore.HasValue) || (!i.HomeScore.HasValue && !i.AwayScore.HasValue))
                    .WithNotification(GenericMessages.FIELDS_CONFLICT, "Informe os dois placares ou nenhum.", nameof(BetItemInput.HomeScore));

            item.RuleFor(i => i.HomeScore)
                .GreaterThanOrEqualTo(0)
                    .WithNotification(GenericMessages.FIELD_INVALID, "Placar não pode ser negativo.", nameof(BetItemInput.HomeScore))
                .When(i => i.HomeScore.HasValue);

            item.RuleFor(i => i.AwayScore)
                .GreaterThanOrEqualTo(0)
                    .WithNotification(GenericMessages.FIELD_INVALID, "Placar não pode ser negativo.", nameof(BetItemInput.AwayScore))
                .When(i => i.AwayScore.HasValue);
        });
    }
}

// --- Handler ---
public sealed class SaveBetsCommandHandler(
    IPoolMemberRepository memberRepository,
    IMatchRepository matchRepository,
    IBetRepository betRepository,
    IUnitOfWork uow,
    IUserContext userContext,
    IMatchScoringService scoringService) : ICommandHandler<SaveBetsCommand, OperationResult>
{
    private const int BET_CUTOFF_MINUTES = 2;

    public async Task<OperationResult> HandleAsync(SaveBetsCommand command, CancellationToken ct)
    {
        var userId = userContext.UserId;

        var isMember = await memberRepository.AnyAsync(
            m => m.PoolId == command.PoolId
                 && m.UserId == userId
                 && m.Status == PoolMemberStatus.Active,
            ct);

        if (!isMember)
            return Result.Warning(GenericMessages.INVALID_OPERATION, "Apenas membros ativos do bolão podem palpitar.");

        var itemsToPersist = command.Items
            .Where(i => i.HomeScore.HasValue && i.AwayScore.HasValue)
            .ToList();

        if (itemsToPersist.Count == 0)
            return Result.Success();

        var matchIds = itemsToPersist.Select(i => i.MatchId).Distinct().ToList();
        var cutoff = DateTime.UtcNow.AddMinutes(BET_CUTOFF_MINUTES);

        var availableMatches = await matchRepository.ToListAsync(
            m => matchIds.Contains(m.Id) && m.PoolId == command.PoolId && m.MatchDate > cutoff,
            @readonly: true,
            ct);

        if (availableMatches.Count != matchIds.Count)
            return Result.Warning(GenericMessages.INVALID_OPERATION, "Há jogos indisponíveis para palpite (já iniciados, bloqueados ou fora do bolão).");

        var existingBets = await betRepository.ToListAsync(
            b => b.UserId == userId && matchIds.Contains(b.MatchId),
            @readonly: false,
            ct);

        var existingByMatch = existingBets.ToDictionary(b => b.MatchId);
        var now = DateTime.UtcNow;

        foreach (var item in itemsToPersist)
        {
            if (existingByMatch.TryGetValue(item.MatchId, out var bet))
            {
                bet.HomeScore = item.HomeScore!.Value;
                bet.AwayScore = item.AwayScore!.Value;
            }
            else
            {
                var newBet = new Bet
                {
                    Id = UIDGen.NewGuid(),
                    MatchId = item.MatchId,
                    UserId = userId,
                    HomeScore = item.HomeScore!.Value,
                    AwayScore = item.AwayScore!.Value,
                    CreatedAt = now,
                };
                await betRepository.AddAsync(newBet, ct);
            }
        }

        var saveResult = await uow.SaveChangesAsync(ct);
        if (!saveResult.IsValid)
            return saveResult;

        foreach (var item in itemsToPersist)
            await scoringService.RecomputeForUserBetAsync(item.MatchId, userId, ct);

        return saveResult;
    }
}
