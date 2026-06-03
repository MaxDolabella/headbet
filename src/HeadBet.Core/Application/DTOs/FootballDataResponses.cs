using System.Text.Json.Serialization;

namespace HeadBet.Core.Application.DTOs;

// --- GET /v4/competitions ---
public class FootballDataCompetitionsResponse
{
    [JsonPropertyName("competitions")]
    public List<FootballDataCompetition> Competitions { get; set; } = [];
}

public class FootballDataCompetition
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("code")]
    public string? Code { get; set; }

    [JsonPropertyName("emblem")]
    public string? Emblem { get; set; }

    [JsonPropertyName("area")]
    public FootballDataArea? Area { get; set; }
}

public class FootballDataArea
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("code")]
    public string? Code { get; set; }

    [JsonPropertyName("flag")]
    public string? Flag { get; set; }
}

// --- GET /v4/competitions/{id}/teams ---
public class FootballDataTeamsResponse
{
    [JsonPropertyName("teams")]
    public List<FootballDataTeam> Teams { get; set; } = [];
}

public class FootballDataTeam
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("tla")]
    public string Tla { get; set; } = string.Empty;

    [JsonPropertyName("crest")]
    public string? Crest { get; set; }
}

// --- GET /v4/competitions/{id}/matches ---
public class FootballDataMatchesResponse
{
    [JsonPropertyName("matches")]
    public List<FootballDataMatch> Matches { get; set; } = [];
}

public class FootballDataMatch
{
    [JsonPropertyName("utcDate")]
    public DateTime UtcDate { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("matchday")]
    public int? Matchday { get; set; }

    [JsonPropertyName("group")]
    public string? Group { get; set; }

    [JsonPropertyName("homeTeam")]
    public FootballDataMatchTeam? HomeTeam { get; set; }

    [JsonPropertyName("awayTeam")]
    public FootballDataMatchTeam? AwayTeam { get; set; }

    [JsonPropertyName("score")]
    public FootballDataMatchScore? Score { get; set; }
}

public class FootballDataMatchTeam
{
    [JsonPropertyName("id")]
    public int? Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("tla")]
    public string? Tla { get; set; }
}

public class FootballDataMatchScore
{
    [JsonPropertyName("fullTime")]
    public FootballDataMatchScoreDetails? FullTime { get; set; }
}

public class FootballDataMatchScoreDetails
{
    [JsonPropertyName("home")]
    public int? Home { get; set; }

    [JsonPropertyName("away")]
    public int? Away { get; set; }
}
