namespace HeadBet.Core.Models;

public class PoolBetsViewModel
{
    public Guid PoolId { get; set; }
    public string PoolName { get; set; } = string.Empty;
    public List<BetItemViewModel> Items { get; set; } = [];
}
