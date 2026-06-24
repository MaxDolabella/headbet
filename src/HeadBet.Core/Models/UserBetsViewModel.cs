namespace HeadBet.Core.Models;

public class UserBetsViewModel
{
    public Guid PoolId { get; set; }
    public string PoolName { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public bool IsCurrentUser { get; set; }
    public bool HasAccess { get; set; }
    public int TotalPoints { get; set; }
    public int BetCount { get; set; }

    // Reusa o item de palpite da tela de estatísticas (jogo, palpite, resultado, pontos).
    public List<StatBetItemViewModel> Bets { get; set; } = [];
}
