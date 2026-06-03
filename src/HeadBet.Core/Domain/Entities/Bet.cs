using Headsoft.Core.Entities;

namespace HeadBet.Core.Domain.Entities;

public class Bet : Entity<Guid>
{
    public Guid MatchId { get; set; }
    public Guid UserId { get; set; }
    public int HomeScore { get; set; }
    public int AwayScore { get; set; }
    public DateTime CreatedAt { get; set; }

    public Match Match { get; set; } = null!;
    public User User { get; set; } = null!;
}
