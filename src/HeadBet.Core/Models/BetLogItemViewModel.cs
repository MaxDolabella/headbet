using HeadBet.Core.Domain.Enums;

namespace HeadBet.Core.Models;

public class BetLogItemViewModel
{
    public Guid PoolId { get; set; }
    public Guid UserId { get; set; }
    public DateTime CreatedAt { get; set; } // BRT
    public string PoolName { get; set; } = string.Empty;
    public string MatchLabel { get; set; } = string.Empty; // "Mandante x Visitante"
    public string UserName { get; set; } = string.Empty;
    public BetLogAction Action { get; set; }
    public int? OldHomeScore { get; set; }
    public int? OldAwayScore { get; set; }
    public int NewHomeScore { get; set; }
    public int NewAwayScore { get; set; }
}
