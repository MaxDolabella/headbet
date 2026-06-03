using HeadBet.Core.Domain.Enums;

namespace HeadBet.Core.Models;

public class MatchScoreFormViewModel
{
    public Guid Id { get; set; }
    public Guid PoolId { get; set; }
    public string HomeTeamName { get; set; } = string.Empty;
    public string HomeTeamAbbreviation { get; set; } = string.Empty;
    public string HomeTeamFlagUrl { get; set; } = string.Empty;
    public string AwayTeamName { get; set; } = string.Empty;
    public string AwayTeamAbbreviation { get; set; } = string.Empty;
    public string AwayTeamFlagUrl { get; set; } = string.Empty;
    public DateTime MatchDate { get; set; }
    public string? Group { get; set; }
    public byte? Round { get; set; }
    public MatchStatus Status { get; set; }

    public int? HomeScore { get; set; }
    public int? AwayScore { get; set; }
}
