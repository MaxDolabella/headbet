namespace HeadBet.Core.Models;

public class PoolBetsViewModel
{
    public Guid PoolId { get; set; }
    public string PoolName { get; set; } = string.Empty;

    /// <summary>Membro é admin do bolão — pode moderar (apagar) comentários dos jogos.</summary>
    public bool IsAdmin { get; set; }

    public List<BetItemViewModel> Items { get; set; } = [];
}
