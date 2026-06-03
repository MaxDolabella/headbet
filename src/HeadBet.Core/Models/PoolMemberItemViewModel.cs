using HeadBet.Core.Domain.Enums;

namespace HeadBet.Core.Models;

public class PoolMemberItemViewModel
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public PoolMemberRole Role { get; set; }
    public PoolMemberStatus Status { get; set; }
    public DateTime JoinedAt { get; set; }
    public bool IsCurrentUser { get; set; }
}
