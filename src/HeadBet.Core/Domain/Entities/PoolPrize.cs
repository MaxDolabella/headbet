using Headsoft.Core.Entities;

namespace HeadBet.Core.Domain.Entities;

public class PoolPrize : Entity
{
    public Guid PoolId { get; set; }
    public int Position { get; set; }
    public decimal? Percentage { get; set; }
    public decimal? FixedAmount { get; set; }

    public Pool Pool { get; set; } = null!;
}
