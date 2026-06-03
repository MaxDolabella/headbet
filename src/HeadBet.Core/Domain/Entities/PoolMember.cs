using Headsoft.Core.Entities;
using HeadBet.Core.Domain.Enums;

namespace HeadBet.Core.Domain.Entities;

public class PoolMember : Entity
{
    public Guid PoolId { get; set; }
    public Guid UserId { get; set; }
    public PoolMemberRole Role { get; set; }
    public PoolMemberStatus Status { get; set; }
    public DateTime JoinedAt { get; set; }

    public Pool Pool { get; set; } = null!;
    public User User { get; set; } = null!;
}
