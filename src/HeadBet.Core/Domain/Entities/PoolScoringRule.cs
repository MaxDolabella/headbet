using Headsoft.Core.Entities;
using HeadBet.Core.Domain.Enums;

namespace HeadBet.Core.Domain.Entities;

public class PoolScoringRule : Entity
{
    public Guid PoolId { get; set; }
    public ScoreType ScoreType { get; set; }
    public int Points { get; set; }

    public Pool Pool { get; set; } = null!;
}
