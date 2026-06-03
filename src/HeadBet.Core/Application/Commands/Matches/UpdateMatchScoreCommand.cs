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
public class UpdateMatchScoreCommand : CommandBase<OperationResult>
{
    public Guid Id { get; set; }
    public Guid PoolId { get; set; }
    public int? HomeScore { get; set; }
    public int? AwayScore { get; set; }

    /// <summary>Quando true, encerra a partida (Status = Finished). Caso contrário, marca como InProgress.</summary>
    public bool Finish { get; set; }
}

// --- Validator ---
public sealed class UpdateMatchScoreCommandValidator : AbstractValidator<UpdateMatchScoreCommand>
{
    public UpdateMatchScoreCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
                .WithNotification(GenericMessages.FIELD_REQUIRED, "Jogo é obrigatório.", nameof(UpdateMatchScoreCommand.Id));

        RuleFor(x => x.PoolId)
            .NotEmpty()
                .WithNotification(GenericMessages.FIELD_REQUIRED, "Pool é obrigatório.", nameof(UpdateMatchScoreCommand.PoolId));

        RuleFor(x => x.HomeScore)
            .NotNull()
                .WithNotification(GenericMessages.FIELD_REQUIRED, "Informe o placar do mandante.", nameof(UpdateMatchScoreCommand.HomeScore))
            .GreaterThanOrEqualTo(0)
                .WithNotification(GenericMessages.FIELD_INVALID, "Gols mandante não pode ser negativo.", nameof(UpdateMatchScoreCommand.HomeScore))
            .When(x => x.HomeScore.HasValue);

        RuleFor(x => x.AwayScore)
            .NotNull()
                .WithNotification(GenericMessages.FIELD_REQUIRED, "Informe o placar do visitante.", nameof(UpdateMatchScoreCommand.AwayScore))
            .GreaterThanOrEqualTo(0)
                .WithNotification(GenericMessages.FIELD_INVALID, "Gols visitante não pode ser negativo.", nameof(UpdateMatchScoreCommand.AwayScore))
            .When(x => x.AwayScore.HasValue);
    }
}

// --- Handler ---
public sealed class UpdateMatchScoreCommandHandler(
    IMatchRepository matchRepository,
    IPoolMemberRepository memberRepository,
    IUnitOfWork uow,
    IUserContext userContext,
    IMatchScoringService scoringService) : ICommandHandler<UpdateMatchScoreCommand, OperationResult>
{
    public async Task<OperationResult> HandleAsync(UpdateMatchScoreCommand command, CancellationToken ct)
    {
        var isAdmin = await memberRepository.AnyAsync(
            m => m.PoolId == command.PoolId
                 && m.UserId == userContext.UserId
                 && m.Role == PoolMemberRole.Admin
                 && m.Status == PoolMemberStatus.Active,
            ct);

        if (!isAdmin)
            return Result.Warning(GenericMessages.INVALID_OPERATION, "Apenas o administrador pode editar o placar.");

        var entity = await matchRepository.GetAsync(m => m.Id == command.Id && m.PoolId == command.PoolId, @readonly: false, ct);
        if (entity is null)
            return Result.Warning(GenericMessages.ITEM_NOT_FOUND, "Jogo não encontrado.");

        if (entity.Status == MatchStatus.Finished)
            return Result.Warning(GenericMessages.INVALID_OPERATION, "Partida já encerrada. Para alterar o placar, use a tela de edição da partida.");

        entity.HomeScore = command.HomeScore;
        entity.AwayScore = command.AwayScore;
        entity.Status = command.Finish ? MatchStatus.Finished : MatchStatus.InProgress;

        var saveResult = await uow.SaveChangesAsync(ct);
        if (saveResult.IsValid)
            await scoringService.RecomputeForMatchAsync(entity.Id, ct);

        return saveResult;
    }
}
