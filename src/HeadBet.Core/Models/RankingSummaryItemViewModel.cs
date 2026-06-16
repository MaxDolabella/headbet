namespace HeadBet.Core.Models;

public class RankingSummaryItemViewModel
{
    public int Position { get; set; }
    public string UserName { get; set; } = string.Empty;
    public int TotalPoints { get; set; }
    public bool IsCurrentUser { get; set; }

    /// <summary>
    /// Variação de posição em relação ao ranking SEM os jogos em andamento.
    /// &gt; 0 subiu, &lt; 0 caiu, 0 manteve (ou não há jogos em andamento).
    /// </summary>
    public int PositionDelta { get; set; }
}
