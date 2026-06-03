using HeadBet.Core.Domain.Enums;

namespace HeadBet.Core.Models;

public class MatchDetailsViewModel
{
    public Guid PoolId { get; set; }
    public string PoolName { get; set; } = string.Empty;
    public bool PoolIsPublic { get; set; }
    public bool IsMember { get; set; }
    public bool IsAdmin { get; set; }

    public Guid MatchId { get; set; }
    public string HomeTeamName { get; set; } = string.Empty;
    public string HomeTeamAbbreviation { get; set; } = string.Empty;
    public string HomeTeamFlagUrl { get; set; } = string.Empty;
    public string AwayTeamName { get; set; } = string.Empty;
    public string AwayTeamAbbreviation { get; set; } = string.Empty;
    public string AwayTeamFlagUrl { get; set; } = string.Empty;
    public DateTime MatchDate { get; set; } // BRT
    public int? HomeScore { get; set; }
    public int? AwayScore { get; set; }
    public MatchStatus Status { get; set; }
    public string? Group { get; set; }
    public byte? Round { get; set; }

    public bool IsBetPeriodOpen { get; set; }
    public bool CanEditMyBet { get; set; }
    public int? MyBetHome { get; set; }
    public int? MyBetAway { get; set; }

    public List<BetListItemViewModel> Bets { get; set; } = [];
}
