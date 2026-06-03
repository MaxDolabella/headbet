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
public class SaveBetCommand : CommandBase<OperationResult>
{
    public Guid PoolId { get; set; }
    public Guid MatchId { get; set; }
    public int? HomeScore { get; set; }
    public int? AwayScore { get; set; }
}

// --- Validator ---
public sealed class SaveBetCommandValidator : AbstractValidator<SaveBetCommand>
{
    public SaveBetCommandValidator()
    {
        RuleFor(x => x.PoolId)
            .NotEmpty()
                .WithNotification(GenericMessages.FIELD_REQUIRED, "Bolão é obrigatório.", nameof(SaveBetCommand.PoolId));

        RuleFor(x => x.MatchId)
            .NotEmpty()
                .WithNotification(GenericMessages.FIELD_REQUIRED, "Jogo é obrigatório.", nameof(SaveBetCommand.MatchId));

        RuleFor(x => x)
            .Must(x => (x.HomeScore.HasValue && x.AwayScore.HasValue) || (!x.HomeScore.HasValue && !x.AwayScore.HasValue))
                .WithNotification(GenericMessages.FIELDS_CONFLICT, "Informe os dois placares ou nenhum.", nameof(SaveBetCommand.HomeScore));

        RuleFor(x => x.HomeScore)
            .GreaterThanOrEqualTo(0)
                .WithNotification(GenericMessages.FIELD_INVALID, "Placar não pode ser negativo.", nameof(SaveBetCommand.HomeScore))
            .When(x => x.HomeScore.HasValue);

        RuleFor(x => x.AwayScore)
            .GreaterThanOrEqualTo(0)
                .WithNotification(GenericMessages.FIELD_INVALID, "Placar não pode ser negativo.", nameof(SaveBetCommand.AwayScore))
            .When(x => x.AwayScore.HasValue);
    }
}

// --- Handler ---
public sealed class SaveBetCommandHandler(
    IPoolMemberRepository memberRepository,
    IMatchRepository matchRepository,
    IBetRepository betRepository,
    IUnitOfWork uow,
    IUserContext userContext,
    IMatchScoringService scoringService) : ICommandHandler<SaveBetCommand, OperationResult>
{
    private const int BET_CUTOFF_MINUTES = 2;

    public async Task<OperationResult> HandleAsync(SaveBetCommand command, CancellationToken ct)
    {
        var userId = userContext.UserId;

        var isMember = await memberRepository.AnyAsync(
            m => m.PoolId == command.PoolId
                 && m.UserId == userId
                 && m.Status == PoolMemberStatus.Active,
            ct);

        if (!isMember)
            return Result.Warning(GenericMessages.INVALID_OPERATION, "Apenas membros ativos do bolão podem palpitar.");

        if (!command.HomeScore.HasValue || !command.AwayScore.HasValue)
            return Result.Warning(GenericMessages.INVALID_OPERATION, "Informe ambos os placares.");

        var cutoff = DateTime.UtcNow.AddMinutes(BET_CUTOFF_MINUTES);

        var match = await matchRepository.GetAsync(
            m => m.Id == command.MatchId && m.PoolId == command.PoolId && m.MatchDate > cutoff,
            @readonly: true,
            ct);

        if (match is null)
            return Result.Warning(GenericMessages.INVALID_OPERATION, "Jogo indisponível para palpite (já iniciado, bloqueado ou fora do bolão).");

        var existingBet = await betRepository.GetAsync(
            b => b.UserId == userId && b.MatchId == command.MatchId,
            @readonly: false,
            ct);

        if (existingBet is not null)
        {
            existingBet.HomeScore = command.HomeScore!.Value;
            existingBet.AwayScore = command.AwayScore!.Value;
        }
        else
        {
            var newBet = new Bet
            {
                Id = UIDGen.NewGuid(),
                MatchId = command.MatchId,
                UserId = userId,
                HomeScore = command.HomeScore!.Value,
                AwayScore = command.AwayScore!.Value,
                CreatedAt = DateTime.UtcNow,
            };
            await betRepository.AddAsync(newBet, ct);
        }

        var saveResult = await uow.SaveChangesAsync(ct);
        if (!saveResult.IsValid)
            return saveResult;

        await scoringService.RecomputeForUserBetAsync(command.MatchId, userId, ct);

        return saveResult;
    }
}
