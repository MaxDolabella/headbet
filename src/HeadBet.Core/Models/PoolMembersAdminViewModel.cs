namespace HeadBet.Core.Models;

public class PoolMembersAdminViewModel
{
    public Guid PoolId { get; set; }
    public string PoolName { get; set; } = string.Empty;
    public List<PoolMemberItemViewModel> Members { get; set; } = [];
}
