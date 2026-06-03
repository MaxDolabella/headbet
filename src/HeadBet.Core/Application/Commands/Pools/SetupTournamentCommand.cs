using System.Text.Json;
using System.Text.RegularExpressions;
using FluentValidation;
using Headsoft.Core;
using Headsoft.Core.Extensions;
using Headsoft.Core.Helpers;
using Headsoft.Core.Interfaces.Data;
using Headsoft.Messaging.Abstractions;
using Headsoft.Messaging.Abstractions.Commands;
using HeadBet.Core.Application.DTOs;
using HeadBet.Core.Domain.Enums;
using HeadBet.Core.Domain.Interfaces;
using HeadBet.Core.Domain.Interfaces.Repositories;
using HeadBet.Core.Infrastructure.Tournament;
using Microsoft.Agents.AI;

namespace HeadBet.Core.Application.Commands;

// --- Command ---
public class SetupTournamentCommand : CommandBase<OperationResult<TournamentSetupResult>>
{
    public Guid Id { get; set; } = UIDGen.NewGuid();
    public Guid PoolId { get; set; }
    public string TournamentName { get; set; } = string.Empty;
    public Guid UserApiKeyId { get; set; }
}

// --- Validator ---
public sealed class SetupTournamentCommandValidator : AbstractValidator<SetupTournamentCommand>
{
    public SetupTournamentCommandValidator()
    {
        RuleFor(x => x.PoolId)
            .NotEmpty()
                .WithNotification(GenericMessages.FIELD_REQUIRED, "Pool é obrigatório.", nameof(SetupTournamentCommand.PoolId));

        RuleFor(x => x.TournamentName)
            .NotEmpty()
                .WithNotification(GenericMessages.FIELD_REQUIRED, "Nome do torneio é obrigatório.", nameof(SetupTournamentCommand.TournamentName))
            .MaximumLength(200)
                .WithNotification(GenericMessages.FIELD_LENGTH, "Nome do torneio deve ter no máximo 200 caracteres.", nameof(SetupTournamentCommand.TournamentName));

        RuleFor(x => x.UserApiKeyId)
            .NotEmpty()
                .WithNotification(GenericMessages.FIELD_REQUIRED, "Selecione uma API Key.", nameof(SetupTournamentCommand.UserApiKeyId));
    }
}

// --- DTO for ProjectTo (internal) ---
internal class UserApiKeyAgentDto
{
    public string ApiKey { get; set; } = string.Empty;
    public AiProvider AiModelProvider { get; set; }
    public string AiModelName { get; set; } = string.Empty;
}

// --- Handler ---
public sealed class SetupTournamentCommandHandler(
    ILogger<SetupTournamentCommandHandler> logger,
    IUserApiKeyRepository apiKeyRepository,
    IPoolMemberRepository memberRepository,
    IUnitOfWork uow,
    IAgentFactory agentFactory,
    IFootballDataClient footballDataClient,
    ITournamentImporter tournamentImporter,
    IUserContext userContext) : ICommandHandler<SetupTournamentCommand, OperationResult<TournamentSetupResult>>
{
    private readonly ILogger _logger = logger;

    private const string STEP1_INSTRUCTIONS = """
        You are a sports data assistant. You will receive a list of football competitions
        from the football-data.org API. Given a tournament name provided by the user,
        identify which competition ID matches the requested tournament.

        Rules:
        - Match by name similarity (e.g. "Copa do Mundo 2026" → "FIFA World Cup")
        - If no exact match exists, pick the closest one
        - If you truly cannot find a match, return -1

        CRITICAL OUTPUT FORMAT:
        Your response MUST be ONLY the competition ID as a plain integer number.
        No markdown, no explanation, no text before or after the number.
        Example: 2001
        """;

    public async Task<OperationResult<TournamentSetupResult>> HandleAsync(
        SetupTournamentCommand command, CancellationToken ct)
    {
        var validationResult = await ValidatePreConditionsAsync(command, ct);
        if (!validationResult.IsValid)
            return validationResult.Result!;

        var keyDto = validationResult.KeyDto!;

        var competitionId = await IdentifyCompetitionAsync(keyDto, command.TournamentName, ct);
        if (competitionId is null)
            return Result.Warning<TournamentSetupResult>(
                GenericMessages.INVALID_OPERATION, "Não foi possível identificar o torneio solicitado.");

        var data = await tournamentImporter.FetchPreviewAsync(competitionId.Value, command.PoolId, ct);
        if (data.Teams.Count == 0 && data.Matches.Count == 0)
            return Result.Warning<TournamentSetupResult>(
                GenericMessages.INVALID_OPERATION, "Nenhum dado novo encontrado para este torneio.");

        await tournamentImporter.ImportAsync(data, command.PoolId, ct);

        var result = await uow.SaveChangesAsync(ct);

        return result.IsValid
            ? Result.Success(data)
            : result.Cast<TournamentSetupResult>();
    }

    private async Task<PreConditionResult> ValidatePreConditionsAsync(
        SetupTournamentCommand command, CancellationToken ct)
    {
        var isAdmin = await memberRepository.AnyAsync(
            m => m.PoolId == command.PoolId
                 && m.UserId == userContext.UserId
                 && m.Role == PoolMemberRole.Admin
                 && m.Status == PoolMemberStatus.Active,
            ct);

        if (!isAdmin)
            return PreConditionResult.Fail(Result.Warning<TournamentSetupResult>(
                GenericMessages.INVALID_OPERATION, "Apenas o administrador pode configurar o torneio."));

        var keyDto = await apiKeyRepository.GetAsync<UserApiKeyAgentDto>(
            k => k.Id == command.UserApiKeyId && k.UserId == userContext.UserId, ct);

        if (keyDto is null)
            return PreConditionResult.Fail(Result.Warning<TournamentSetupResult>(
                GenericMessages.ITEM_NOT_FOUND, "API Key não encontrada."));

        return PreConditionResult.Ok(keyDto);
    }

    private async Task<int?> IdentifyCompetitionAsync(
        UserApiKeyAgentDto keyDto, string tournamentName, CancellationToken ct)
    {
        var competitions = await footballDataClient.GetCompetitionsAsync(ct);
        var competitionsJson = JsonSerializer.Serialize(
            competitions.Competitions.Select(c => new { c.Id, c.Name }));

        var agent = agentFactory.Create(
            keyDto.AiModelProvider, keyDto.AiModelName, keyDto.ApiKey, STEP1_INSTRUCTIONS);

        var prompt = $"""
            Tournament requested: "{tournamentName}"

            Available competitions:
            {competitionsJson}
            """;

        var response = await agent.RunAsync(prompt, cancellationToken: ct);
        var text = StripMarkdown(response.ToString());

        _logger.LogInformation("AI identified competition ID '{Text}' for tournament '{TournamentName}'", text, tournamentName);

        return int.TryParse(text.Trim(), out var id) && id >= 0 ? id : null;
    }

    private static string StripMarkdown(string text)
    {
        var trimmed = text.Trim();
        var match = Regex.Match(trimmed, @"```(?:\w+)?\s*([\s\S]*?)```");
        return match.Success ? match.Groups[1].Value.Trim() : trimmed;
    }

    private record PreConditionResult(
        bool IsValid,
        OperationResult<TournamentSetupResult>? Result,
        UserApiKeyAgentDto? KeyDto)
    {
        public static PreConditionResult Ok(UserApiKeyAgentDto keyDto) => new(true, null, keyDto);
        public static PreConditionResult Fail(OperationResult<TournamentSetupResult> result) => new(false, result, null);
    }
}
