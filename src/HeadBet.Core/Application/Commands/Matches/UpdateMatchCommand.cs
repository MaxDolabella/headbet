using AutoMapper;
using FluentValidation;
using Headsoft.Core;
using Headsoft.Core.Extensions;
using Headsoft.Core.Interfaces.Data;
using Headsoft.Messaging.Abstractions;
using Headsoft.Messaging.Abstractions.Commands;
using HeadBet.Core.Domain.Enums;
using HeadBet.Core.Domain.Interfaces;
using HeadBet.Core.Domain.Interfaces.Repositories;
using HeadBet.Core.Extensions;

namespace HeadBet.Core.Application.Commands;

// --- Command ---
public class UpdateMatchCommand : CommandBase<OperationResult>
{
    public Guid Id { get; set; }
    public Guid PoolId { get; set; }
    public Guid HomeTeamId { get; set; }
    public Guid AwayTeamId { get; set; }
    public DateTime MatchDate { get; set; }
    public int? HomeScore { get; set; }
    public int? AwayScore { get; set; }
    public MatchStatus Status { get; set; }
    public string? Group { get; set; }
    public byte Round { get; set; }
    public string? BroadcastUrl { get; set; }
}

// --- Validator ---
public sealed class UpdateMatchCommandValidator : AbstractValidator<UpdateMatchCommand>
{
    public UpdateMatchCommandValidator(ITeamRepository teamRepository)
    {
        RuleFor(x => x.HomeTeamId)
            .NotEmpty()
                .WithNotification(GenericMessages.FIELD_REQUIRED, "Time da casa é obrigatório.", nameof(UpdateMatchCommand.HomeTeamId));

        RuleFor(x => x.AwayTeamId)
            .NotEmpty()
                .WithNotification(GenericMessages.FIELD_REQUIRED, "Time visitante é obrigatório.", nameof(UpdateMatchCommand.AwayTeamId));

        RuleFor(x => x)
            .Must(cmd => cmd.HomeTeamId != cmd.AwayTeamId)
                .WithNotification(GenericMessages.FIELDS_CONFLICT, "Time da casa e visitante devem ser diferentes.", nameof(UpdateMatchCommand.AwayTeamId))
            .When(x => x.HomeTeamId != Guid.Empty && x.AwayTeamId != Guid.Empty);

        RuleFor(x => x)
            .MustAsync(async (cmd, ct) => await teamRepository.AnyAsync(t => t.Id == cmd.HomeTeamId && t.PoolId == cmd.PoolId, ct))
                .WithNotification(GenericMessages.ITEM_NOT_FOUND, "Time da casa não pertence a este bolão.", nameof(UpdateMatchCommand.HomeTeamId))
            .When(x => x.HomeTeamId != Guid.Empty);

        RuleFor(x => x)
            .MustAsync(async (cmd, ct) => await teamRepository.AnyAsync(t => t.Id == cmd.AwayTeamId && t.PoolId == cmd.PoolId, ct))
                .WithNotification(GenericMessages.ITEM_NOT_FOUND, "Time visitante não pertence a este bolão.", nameof(UpdateMatchCommand.AwayTeamId))
            .When(x => x.AwayTeamId != Guid.Empty);

        RuleFor(x => x.MatchDate)
            .NotEmpty()
                .WithNotification(GenericMessages.FIELD_REQUIRED, "Data/hora do jogo é obrigatória.", nameof(UpdateMatchCommand.MatchDate));

        RuleFor(x => x.Group)
            .MaximumLength(50)
                .WithNotification(GenericMessages.FIELD_LENGTH, "Grupo deve ter no máximo 50 caracteres.", nameof(UpdateMatchCommand.Group));

        RuleFor(x => x.BroadcastUrl)
            .MaximumLength(500)
                .WithNotification(GenericMessages.FIELD_LENGTH, "Link da transmissão deve ter no máximo 500 caracteres.", nameof(UpdateMatchCommand.BroadcastUrl))
            .Must(url => YouTubeUrl.TryGetVideoId(url, out _))
                .WithNotification(GenericMessages.FIELD_INVALID, "Link da transmissão deve ser um link de vídeo válido do YouTube.", nameof(UpdateMatchCommand.BroadcastUrl))
            .When(x => !string.IsNullOrWhiteSpace(x.BroadcastUrl));
    }
}

// --- Handler ---
public sealed class UpdateMatchCommandHandler(
    IMatchRepository matchRepository,
    IPoolMemberRepository memberRepository,
    IUnitOfWork uow,
    IMapper mapper,
    IUserContext userContext,
    IMatchScoringService scoringService) : ICommandHandler<UpdateMatchCommand, OperationResult>
{
    public async Task<OperationResult> HandleAsync(UpdateMatchCommand command, CancellationToken ct)
    {
        var isAdmin = await memberRepository.AnyAsync(
            m => m.PoolId == command.PoolId
                 && m.UserId == userContext.UserId
                 && m.Role == PoolMemberRole.Admin
                 && m.Status == PoolMemberStatus.Active,
            ct);

        if (!isAdmin)
            return Result.Warning(GenericMessages.INVALID_OPERATION, "Apenas o administrador pode editar jogos.");

        var entity = await matchRepository.GetAsync(m => m.Id == command.Id && m.PoolId == command.PoolId, @readonly: false, ct);
        if (entity is null)
            return Result.Warning(GenericMessages.ITEM_NOT_FOUND, "Jogo não encontrado.");

        mapper.Map(command, entity);
        entity.MatchDate = entity.MatchDate.ToUtcFromBrt();
        var saveResult = await uow.SaveChangesAsync(ct);
        if (saveResult.IsValid)
            await scoringService.RecomputeForMatchAsync(entity.Id, ct);

        return saveResult;
    }
}
