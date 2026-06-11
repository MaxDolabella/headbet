using Headsoft.Core.Entities;
using HeadBet.Core.Domain.Enums;

namespace HeadBet.Core.Domain.Entities;

public class Match : Entity<Guid>
{
    public Guid PoolId { get; set; }
    public Guid HomeTeamId { get; set; }
    public Guid AwayTeamId { get; set; }
    public DateTime MatchDate { get; set; }
    public int? HomeScore { get; set; }
    public int? AwayScore { get; set; }
    public MatchStatus Status { get; set; }
    public string? Group { get; set; }
    public byte? Round { get; set; }
    public string? BroadcastUrl { get; set; }

    public Pool Pool { get; set; } = null!;
    public Team HomeTeam { get; set; } = null!;
    public Team AwayTeam { get; set; } = null!;
}
