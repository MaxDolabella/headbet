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
using HeadBet.Core.Extensions;

namespace HeadBet.Core.Application.Commands;

// --- Command ---
public class CreateMatchCommand : CommandBase<OperationResult<Guid>>
{
    public Guid Id { get; set; } = UIDGen.NewGuid();
    public Guid PoolId { get; set; }
    public Guid HomeTeamId { get; set; }
    public Guid AwayTeamId { get; set; }
    public DateTime MatchDate { get; set; }
    public int? HomeScore { get; set; }
    public int? AwayScore { get; set; }
    public MatchStatus Status { get; set; } = MatchStatus.Scheduled;
    public string? Group { get; set; }
    public byte Round { get; set; }
}

// --- Validator ---
public sealed class CreateMatchCommandValidator : AbstractValidator<CreateMatchCommand>
{
    public CreateMatchCommandValidator(ITeamRepository teamRepository)
    {
        RuleFor(x => x.PoolId)
            .NotEmpty()
                .WithNotification(GenericMessages.FIELD_REQUIRED, "Pool é obrigatório.", nameof(CreateMatchCommand.PoolId));

        RuleFor(x => x.HomeTeamId)
            .NotEmpty()
                .WithNotification(GenericMessages.FIELD_REQUIRED, "Time da casa é obrigatório.", nameof(CreateMatchCommand.HomeTeamId));

        RuleFor(x => x.AwayTeamId)
            .NotEmpty()
                .WithNotification(GenericMessages.FIELD_REQUIRED, "Time visitante é obrigatório.", nameof(CreateMatchCommand.AwayTeamId));

        RuleFor(x => x)
            .Must(cmd => cmd.HomeTeamId != cmd.AwayTeamId)
                .WithNotification(GenericMessages.FIELDS_CONFLICT, "Time da casa e visitante devem ser diferentes.", nameof(CreateMatchCommand.AwayTeamId))
            .When(x => x.HomeTeamId != Guid.Empty && x.AwayTeamId != Guid.Empty);

        RuleFor(x => x)
            .MustAsync(async (cmd, ct) => await teamRepository.AnyAsync(t => t.Id == cmd.HomeTeamId && t.PoolId == cmd.PoolId, ct))
                .WithNotification(GenericMessages.ITEM_NOT_FOUND, "Time da casa não pertence a este bolão.", nameof(CreateMatchCommand.HomeTeamId))
            .When(x => x.HomeTeamId != Guid.Empty);

        RuleFor(x => x)
            .MustAsync(async (cmd, ct) => await teamRepository.AnyAsync(t => t.Id == cmd.AwayTeamId && t.PoolId == cmd.PoolId, ct))
                .WithNotification(GenericMessages.ITEM_NOT_FOUND, "Time visitante não pertence a este bolão.", nameof(CreateMatchCommand.AwayTeamId))
            .When(x => x.AwayTeamId != Guid.Empty);

        RuleFor(x => x.MatchDate)
            .NotEmpty()
                .WithNotification(GenericMessages.FIELD_REQUIRED, "Data/hora do jogo é obrigatória.", nameof(CreateMatchCommand.MatchDate));

        RuleFor(x => x.Group)
            .MaximumLength(50)
                .WithNotification(GenericMessages.FIELD_LENGTH, "Grupo deve ter no máximo 50 caracteres.", nameof(CreateMatchCommand.Group));
    }
}

// --- Handler ---
public sealed class CreateMatchCommandHandler(
    IMatchRepository matchRepository,
    IPoolMemberRepository memberRepository,
    IUnitOfWork uow,
    IMapper mapper,
    IUserContext userContext) : ICommandHandler<CreateMatchCommand, OperationResult<Guid>>
{
    public async Task<OperationResult<Guid>> HandleAsync(CreateMatchCommand command, CancellationToken ct)
    {
        var isAdmin = await memberRepository.AnyAsync(
            m => m.PoolId == command.PoolId
                 && m.UserId == userContext.UserId
                 && m.Role == PoolMemberRole.Admin
                 && m.Status == PoolMemberStatus.Active,
            ct);

        if (!isAdmin)
            return Result.Warning<Guid>(GenericMessages.INVALID_OPERATION, "Apenas o administrador pode cadastrar jogos.");

        var entity = mapper.Map<Match>(command);
        entity.MatchDate = entity.MatchDate.ToUtcFromBrt();

        await matchRepository.AddAsync(entity, ct);
        var result = await uow.SaveChangesAsync(ct);

        return result.IsValid
            ? Result.Success(entity.Id)
            : result.Cast<Guid>();
    }
}
