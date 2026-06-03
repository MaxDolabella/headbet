using Headsoft.Core.Data.Extensions;
using Headsoft.Core.Helpers;
using HeadBet.Core.Application.DTOs;
using HeadBet.Core.Domain.Entities;
using HeadBet.Core.Domain.Enums;
using HeadBet.Core.Domain.Interfaces;
using HeadBet.Core.Domain.Interfaces.Repositories;

namespace HeadBet.Core.Infrastructure.Tournament;

public sealed class TournamentImporter(
    ILogger<TournamentImporter> logger,
    IFootballDataClient footballDataClient,
    ITeamRepository teamRepository,
    IMatchRepository matchRepository) : ITournamentImporter
{
    private const string MATCH_STATUS_TIMED = "TIMED";
    private const string MATCH_STATUS_FINISHED = "FINISHED";

    public async Task<TournamentSetupResult> FetchPreviewAsync(int competitionId, Guid poolId, CancellationToken ct)
    {
        var teamsTask = footballDataClient.GetTeamsAsync(competitionId, ct);
        var matchesTask = footballDataClient.GetMatchesAsync(competitionId, ct);
        await Task.WhenAll(teamsTask, matchesTask);

        var relevantMatches = matchesTask.Result.Matches
            .Where(m => (string.Equals(m.Status, MATCH_STATUS_TIMED, StringComparison.OrdinalIgnoreCase)
                      || string.Equals(m.Status, MATCH_STATUS_FINISHED, StringComparison.OrdinalIgnoreCase))
                     && m.HomeTeam?.Id != null
                     && m.AwayTeam?.Id != null)
            .ToList();

        logger.LogInformation(
            "Fetched {MatchCount} matches from API for competition {CompetitionId}",
            relevantMatches.Count, competitionId);

        var existingTeams = await teamRepository.ToListAsync<TeamKeyDto>(t => t.PoolId == poolId, ct);
        if (existingTeams.Count > 0)
        {
            var teamIdToExtId = existingTeams
                .Where(t => t.ExternalId.HasValue)
                .ToDictionary(t => t.Id, t => t.ExternalId!.Value);

            var existingMatches = await matchRepository.ToListAsync<MatchKeyDto>(m => m.PoolId == poolId, ct);
            var existingKeys = existingMatches
                .Where(m => teamIdToExtId.ContainsKey(m.HomeTeamId) && teamIdToExtId.ContainsKey(m.AwayTeamId))
                .Select(m => (teamIdToExtId[m.HomeTeamId], teamIdToExtId[m.AwayTeamId], m.MatchDate))
                .ToHashSet();

            relevantMatches = relevantMatches
                .Where(m => !existingKeys.Contains((m.HomeTeam!.Id!.Value, m.AwayTeam!.Id!.Value, m.UtcDate)))
                .ToList();
        }

        if (relevantMatches.Count == 0)
            return new TournamentSetupResult();

        var apiTeamsById = teamsTask.Result.Teams.ToDictionary(t => t.Id);

        var teams = relevantMatches
            .SelectMany(m => new[] { m.HomeTeam!, m.AwayTeam! })
            .DistinctBy(t => t.Id)
            .Select(matchTeam =>
            {
                var apiTeam = apiTeamsById.GetValueOrDefault(matchTeam.Id!.Value);
                return new TournamentTeamData
                {
                    ExternalId = matchTeam.Id!.Value,
                    Name = apiTeam?.Name ?? matchTeam.Name ?? string.Empty,
                    Abbreviation = matchTeam.Tla ?? apiTeam?.Tla ?? string.Empty,
                    FlagUrl = apiTeam?.Crest,
                };
            })
            .ToList();

        var matches = relevantMatches
            .OrderBy(m => m.UtcDate)
            .Select(m => new TournamentMatchData
            {
                HomeTeamExternalId = m.HomeTeam!.Id!.Value,
                AwayTeamExternalId = m.AwayTeam!.Id!.Value,
                HomeTeamAbbreviation = m.HomeTeam!.Tla ?? string.Empty,
                AwayTeamAbbreviation = m.AwayTeam!.Tla ?? string.Empty,
                MatchDateUtc = m.UtcDate,
                Group = m.Group,
                Round = m.Matchday.HasValue ? (byte)m.Matchday.Value : null,
                Status = string.Equals(m.Status, MATCH_STATUS_FINISHED, StringComparison.OrdinalIgnoreCase)
                    ? "Finished"
                    : "Scheduled",
                HomeScore = m.Score?.FullTime?.Home,
                AwayScore = m.Score?.FullTime?.Away,
            })
            .ToList();

        return new TournamentSetupResult { Teams = teams, Matches = matches };
    }

    public async Task ImportAsync(TournamentSetupResult data, Guid poolId, CancellationToken ct)
    {
        var existingTeams = await teamRepository.ToListAsync<TeamKeyDto>(t => t.PoolId == poolId, ct);
        var teamMap = new Dictionary<int, Guid>();
        foreach (var t in existingTeams.Where(t => t.ExternalId.HasValue))
            teamMap[t.ExternalId!.Value] = t.Id;

        foreach (var t in data.Teams)
        {
            if (teamMap.ContainsKey(t.ExternalId))
                continue;

            var teamId = UIDGen.NewGuid();
            teamMap[t.ExternalId] = teamId;

            await teamRepository.AddAsync(new Team
            {
                Id = teamId,
                PoolId = poolId,
                ExternalId = t.ExternalId,
                Name = t.Name,
                Abbreviation = t.Abbreviation,
                FlagUrl = t.FlagUrl,
            }, ct);
        }

        foreach (var m in data.Matches.OrderBy(m => m.MatchDateUtc))
        {
            if (!teamMap.TryGetValue(m.HomeTeamExternalId, out var homeId)
                || !teamMap.TryGetValue(m.AwayTeamExternalId, out var awayId))
                continue;

            var status = string.Equals(m.Status, "Finished", StringComparison.OrdinalIgnoreCase)
                ? MatchStatus.Finished
                : MatchStatus.Scheduled;

            await matchRepository.AddAsync(new Domain.Entities.Match
            {
                Id = UIDGen.NewGuid(),
                PoolId = poolId,
                HomeTeamId = homeId,
                AwayTeamId = awayId,
                MatchDate = m.MatchDateUtc,
                HomeScore = m.HomeScore,
                AwayScore = m.AwayScore,
                Status = status,
                Group = m.Group,
                Round = m.Round,
            }, ct);
        }
    }

}
