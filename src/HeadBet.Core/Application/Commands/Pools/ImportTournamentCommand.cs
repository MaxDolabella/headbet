using FluentValidation;
using Headsoft.Core;
using Headsoft.Core.Extensions;
using Headsoft.Core.Interfaces.Data;
using Headsoft.Messaging.Abstractions;
using Headsoft.Messaging.Abstractions.Commands;
using HeadBet.Core.Application.DTOs;
using HeadBet.Core.Domain.Enums;
using HeadBet.Core.Domain.Interfaces;
using HeadBet.Core.Domain.Interfaces.Repositories;
using HeadBet.Core.Infrastructure.Tournament;

namespace HeadBet.Core.Application.Commands;

// --- Command ---
public class ImportTournamentCommand : CommandBase<OperationResult<TournamentSetupResult>>
{
    public Guid PoolId { get; set; }
    public int CompetitionId { get; set; }
}

// --- Validator ---
public sealed class ImportTournamentCommandValidator : AbstractValidator<ImportTournamentCommand>
{
    public ImportTournamentCommandValidator()
    {
        RuleFor(x => x.PoolId)
            .NotEmpty()
                .WithNotification(GenericMessages.FIELD_REQUIRED, "Pool é obrigatório.", nameof(ImportTournamentCommand.PoolId));

        RuleFor(x => x.CompetitionId)
            .GreaterThan(0)
                .WithNotification(GenericMessages.FIELD_REQUIRED, "Competição é obrigatória.", nameof(ImportTournamentCommand.CompetitionId));
    }
}

// --- Handler ---
public sealed class ImportTournamentCommandHandler(
    IPoolMemberRepository memberRepository,
    ITournamentImporter tournamentImporter,
    IUnitOfWork uow,
    IUserContext userContext) : ICommandHandler<ImportTournamentCommand, OperationResult<TournamentSetupResult>>
{
    public async Task<OperationResult<TournamentSetupResult>> HandleAsync(
        ImportTournamentCommand command, CancellationToken ct)
    {
        var isAdmin = await memberRepository.AnyAsync(
            m => m.PoolId == command.PoolId
                 && m.UserId == userContext.UserId
                 && m.Role == PoolMemberRole.Admin
                 && m.Status == PoolMemberStatus.Active,
            ct);

        if (!isAdmin)
            return Result.Warning<TournamentSetupResult>(
                GenericMessages.INVALID_OPERATION, "Apenas o administrador pode configurar o torneio.");

        var data = await tournamentImporter.FetchPreviewAsync(command.CompetitionId, command.PoolId, ct);
        if (data.Teams.Count == 0 && data.Matches.Count == 0)
            return Result.Warning<TournamentSetupResult>(
                GenericMessages.INVALID_OPERATION, "Nenhum dado novo encontrado para esta competição.");

        await tournamentImporter.ImportAsync(data, command.PoolId, ct);

        var result = await uow.SaveChangesAsync(ct);

        return result.IsValid
            ? Result.Success(data)
            : result.Cast<TournamentSetupResult>();
    }
}
