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

namespace HeadBet.Core.Application.Commands;

// --- Command ---
public class CreateTeamCommand : CommandBase<OperationResult<Guid>>
{
    public Guid Id { get; set; } = UIDGen.NewGuid();
    public Guid PoolId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Abbreviation { get; set; } = string.Empty;
    public string? FlagUrl { get; set; }
}

// --- Validator ---
public sealed class CreateTeamCommandValidator : AbstractValidator<CreateTeamCommand>
{
    public CreateTeamCommandValidator(ITeamRepository teamRepository)
    {
        RuleFor(x => x.PoolId)
            .NotEmpty()
                .WithNotification(GenericMessages.FIELD_REQUIRED, "Pool é obrigatório.", nameof(CreateTeamCommand.PoolId));

        RuleFor(x => x.Name)
            .NotEmpty()
                .WithNotification(GenericMessages.FIELD_REQUIRED, "Nome é obrigatório.", nameof(CreateTeamCommand.Name))
            .MaximumLength(100)
                .WithNotification(GenericMessages.FIELD_LENGTH, "Nome deve ter no máximo 100 caracteres.", nameof(CreateTeamCommand.Name));

        RuleFor(x => x.Abbreviation)
            .NotEmpty()
                .WithNotification(GenericMessages.FIELD_REQUIRED, "Abreviação é obrigatória.", nameof(CreateTeamCommand.Abbreviation))
            .MaximumLength(10)
                .WithNotification(GenericMessages.FIELD_LENGTH, "Abreviação deve ter no máximo 10 caracteres.", nameof(CreateTeamCommand.Abbreviation));

        RuleFor(x => x)
            .MustAsync(async (cmd, ct) => !await teamRepository.AnyAsync(
                t => t.PoolId == cmd.PoolId && t.Abbreviation == cmd.Abbreviation, ct))
                .WithNotification(GenericMessages.FIELD_UNIQUE, "Já existe um time com esta abreviação neste bolão.", nameof(CreateTeamCommand.Abbreviation))
            .When(x => !string.IsNullOrWhiteSpace(x.Abbreviation));

        RuleFor(x => x.FlagUrl)
            .MaximumLength(500)
                .WithNotification(GenericMessages.FIELD_LENGTH, "URL deve ter no máximo 500 caracteres.", nameof(CreateTeamCommand.FlagUrl));
    }
}

// --- Handler ---
public sealed class CreateTeamCommandHandler(
    ITeamRepository teamRepository,
    IPoolMemberRepository memberRepository,
    IUnitOfWork uow,
    IMapper mapper,
    IUserContext userContext) : ICommandHandler<CreateTeamCommand, OperationResult<Guid>>
{
    public async Task<OperationResult<Guid>> HandleAsync(CreateTeamCommand command, CancellationToken ct)
    {
        var isAdmin = await memberRepository.AnyAsync(
            m => m.PoolId == command.PoolId
                 && m.UserId == userContext.UserId
                 && m.Role == PoolMemberRole.Admin
                 && m.Status == PoolMemberStatus.Active,
            ct);

        if (!isAdmin)
            return Result.Warning<Guid>(GenericMessages.INVALID_OPERATION, "Apenas o administrador pode cadastrar times.");

        var entity = mapper.Map<Team>(command);

        await teamRepository.AddAsync(entity, ct);
        var result = await uow.SaveChangesAsync(ct);

        return result.IsValid
            ? Result.Success(entity.Id)
            : result.Cast<Guid>();
    }
}
