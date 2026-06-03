namespace HeadBet.Core.Models;

public class SetupWizardPreviewViewModel
{
    public Guid PoolId { get; set; }
    public string PoolName { get; set; } = string.Empty;
    public int CompetitionId { get; set; }
    public string CompetitionName { get; set; } = string.Empty;
    public string? CompetitionEmblemUrl { get; set; }
    public string? AreaName { get; set; }

    public int TeamCount { get; set; }
    public int MatchCount { get; set; }
    public DateTime? FirstMatchDateBrt { get; set; }
    public DateTime? LastMatchDateBrt { get; set; }

    public List<TeamPreviewRow> Teams { get; set; } = [];
    public List<MatchPreviewRow> Matches { get; set; } = [];
}

public class TeamPreviewRow
{
    public string Name { get; set; } = string.Empty;
    public string Abbreviation { get; set; } = string.Empty;
    public string? FlagUrl { get; set; }
}

public class MatchPreviewRow
{
    public DateTime MatchDateBrt { get; set; }
    public string HomeTeamAbbreviation { get; set; } = string.Empty;
    public string AwayTeamAbbreviation { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int? HomeScore { get; set; }
    public int? AwayScore { get; set; }
    public string? Group { get; set; }
    public byte? Round { get; set; }
}
