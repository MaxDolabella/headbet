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

namespace HeadBet.Core.Application.Commands;

// --- Command ---
public class UpdateTeamCommand : CommandBase<OperationResult>
{
    public Guid Id { get; set; }
    public Guid PoolId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Abbreviation { get; set; } = string.Empty;
    public string? FlagUrl { get; set; }
}

// --- Validator ---
public sealed class UpdateTeamCommandValidator : AbstractValidator<UpdateTeamCommand>
{
    public UpdateTeamCommandValidator(ITeamRepository teamRepository)
    {
        RuleFor(x => x.Name)
            .NotEmpty()
                .WithNotification(GenericMessages.FIELD_REQUIRED, "Nome é obrigatório.", nameof(UpdateTeamCommand.Name))
            .MaximumLength(100)
                .WithNotification(GenericMessages.FIELD_LENGTH, "Nome deve ter no máximo 100 caracteres.", nameof(UpdateTeamCommand.Name));

        RuleFor(x => x.Abbreviation)
            .NotEmpty()
                .WithNotification(GenericMessages.FIELD_REQUIRED, "Abreviação é obrigatória.", nameof(UpdateTeamCommand.Abbreviation))
            .MaximumLength(10)
                .WithNotification(GenericMessages.FIELD_LENGTH, "Abreviação deve ter no máximo 10 caracteres.", nameof(UpdateTeamCommand.Abbreviation));

        RuleFor(x => x)
            .MustAsync(async (cmd, ct) => !await teamRepository.AnyAsync(
                t => t.PoolId == cmd.PoolId && t.Abbreviation == cmd.Abbreviation && t.Id != cmd.Id, ct))
                .WithNotification(GenericMessages.FIELD_UNIQUE, "Já existe um time com esta abreviação neste bolão.", nameof(UpdateTeamCommand.Abbreviation))
            .When(x => !string.IsNullOrWhiteSpace(x.Abbreviation));

        RuleFor(x => x.FlagUrl)
            .MaximumLength(500)
                .WithNotification(GenericMessages.FIELD_LENGTH, "URL deve ter no máximo 500 caracteres.", nameof(UpdateTeamCommand.FlagUrl));
    }
}

// --- Handler ---
public sealed class UpdateTeamCommandHandler(
    ITeamRepository teamRepository,
    IPoolMemberRepository memberRepository,
    IUnitOfWork uow,
    IMapper mapper,
    IUserContext userContext) : ICommandHandler<UpdateTeamCommand, OperationResult>
{
    public async Task<OperationResult> HandleAsync(UpdateTeamCommand command, CancellationToken ct)
    {
        var isAdmin = await memberRepository.AnyAsync(
            m => m.PoolId == command.PoolId
                 && m.UserId == userContext.UserId
                 && m.Role == PoolMemberRole.Admin
                 && m.Status == PoolMemberStatus.Active,
            ct);

        if (!isAdmin)
            return Result.Warning(GenericMessages.INVALID_OPERATION, "Apenas o administrador pode editar times.");

        var entity = await teamRepository.GetAsync(t => t.Id == command.Id && t.PoolId == command.PoolId, @readonly: false, ct);
        if (entity is null)
            return Result.Warning(GenericMessages.ITEM_NOT_FOUND, "Time não encontrado.");

        mapper.Map(command, entity);
        return await uow.SaveChangesAsync(ct);
    }
}
