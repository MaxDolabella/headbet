using Headsoft.Core.Entities;
using HeadBet.Core.Domain.Enums;

namespace HeadBet.Core.Domain.Entities;

public class MatchUserScore : Entity<Guid>
{
    public Guid MatchId { get; set; }
    public Guid UserId { get; set; }
    public int Points { get; set; }
    public ScoreType AppliedRule { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Match Match { get; set; } = null!;
    public User User { get; set; } = null!;
}
