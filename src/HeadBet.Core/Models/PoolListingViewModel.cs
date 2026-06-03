namespace HeadBet.Core.Models;

public class PoolListingViewModel
{
    public List<PoolListViewModel> Mine { get; set; } = [];
    public List<PoolListViewModel> Public { get; set; } = [];
}
