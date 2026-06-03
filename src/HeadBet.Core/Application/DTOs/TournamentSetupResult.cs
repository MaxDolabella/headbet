using System.ComponentModel;

namespace HeadBet.Core.Application.DTOs;

/// <summary>
/// Structured output returned by the AI agent when setting up a tournament.
/// </summary>
public class TournamentSetupResult
{
    [Description("List of teams participating in the tournament")]
    public List<TournamentTeamData> Teams { get; set; } = [];

    [Description("List of matches scheduled for the tournament, ordered by date ascending")]
    public List<TournamentMatchData> Matches { get; set; } = [];
}

public class TournamentTeamData
{
    public int ExternalId { get; set; }

    [Description("Full team name in Brazilian Portuguese (e.g. 'Brasil', 'Alemanha')")]
    public string Name { get; set; } = string.Empty;

    [Description("FIFA 3-letter abbreviation (e.g. 'BRA', 'GER')")]
    public string Abbreviation { get; set; } = string.Empty;

    [Description("Flag/badge URL: national teams use flagcdn.com, clubs use api-sports.io")]
    public string? FlagUrl { get; set; }
}

public class TournamentMatchData
{
    public int HomeTeamExternalId { get; set; }
    public int AwayTeamExternalId { get; set; }

    [Description("FIFA 3-letter abbreviation of the home team")]
    public string HomeTeamAbbreviation { get; set; } = string.Empty;

    [Description("FIFA 3-letter abbreviation of the away team")]
    public string AwayTeamAbbreviation { get; set; } = string.Empty;

    [Description("Match date and time in UTC (ISO 8601 format)")]
    public DateTime MatchDateUtc { get; set; }

    [Description("Group/round name in pt-BR (e.g. 'Grupo A', 'Oitavas de Final', 'Final')")]
    public string? Group { get; set; }

    [Description("Round number mapped from matchday. Null when matchday is not defined.")]
    public byte? Round { get; set; }

    [Description("Match status: 'Scheduled' for future matches, 'Finished' for completed matches")]
    public string Status { get; set; } = "Scheduled";

    [Description("Home team score (null for scheduled matches, integer for finished)")]
    public int? HomeScore { get; set; }

    [Description("Away team score (null for scheduled matches, integer for finished)")]
    public int? AwayScore { get; set; }
}
