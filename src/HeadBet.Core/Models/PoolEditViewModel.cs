using HeadBet.Core.Domain.Enums;

namespace HeadBet.Core.Models;

public class PoolEditViewModel : IPrizesCardModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsPaid { get; set; }
    public decimal? EntryFee { get; set; }
    public decimal CollectedAmount { get; set; }
    public bool AutoAccept { get; set; }
    public bool IsActive { get; set; }
    public bool IsPublic { get; set; }
    public PrizeMode PrizeMode { get; set; } = PrizeMode.Percentage;
    public List<PrizeItemViewModel> Prizes { get; set; } = [];
    public decimal SimulatedTotal { get; set; }
}
