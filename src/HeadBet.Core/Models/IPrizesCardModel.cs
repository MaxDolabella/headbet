using HeadBet.Core.Domain.Enums;

namespace HeadBet.Core.Models;

public interface IPrizesCardModel
{
    PrizeMode PrizeMode { get; set; }
    List<PrizeItemViewModel> Prizes { get; set; }
    decimal SimulatedTotal { get; set; }
}
