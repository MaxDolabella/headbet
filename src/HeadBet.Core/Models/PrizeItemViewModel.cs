namespace HeadBet.Core.Models;

public class PrizeItemViewModel
{
    public int Position { get; set; }
    public decimal? Percentage { get; set; }
    public decimal? FixedAmount { get; set; }
    public decimal CalculatedAmount { get; set; }
}
