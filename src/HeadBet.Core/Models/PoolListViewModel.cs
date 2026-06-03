using HeadBet.Core.Domain.Enums;

namespace HeadBet.Core.Models;

public class PoolListViewModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsPaid { get; set; }
    public decimal? EntryFee { get; set; }
    public bool IsActive { get; set; }
    public bool IsPublic { get; set; }
    public int MemberCount { get; set; }
    public PoolMemberRole MyRole { get; set; }
    public PoolMemberStatus? MyStatus { get; set; }
}
