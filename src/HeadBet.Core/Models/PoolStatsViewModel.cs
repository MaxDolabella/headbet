using HeadBet.Core.Domain.Enums;

namespace HeadBet.Core.Models;

public class PoolStatsViewModel
{
    public Guid PoolId { get; set; }
    public string PoolName { get; set; } = string.Empty;
    public bool HasData { get; set; }

    public List<StatBetItemViewModel> Bets { get; set; } = [];
    public List<GameHighlightViewModel> MostExact { get; set; } = [];
    public List<GameHighlightViewModel> TopAverage { get; set; } = [];
    public List<GameHighlightViewModel> BottomAverage { get; set; } = [];
    public List<ConsensusRowViewModel> Consensus { get; set; } = [];
}

// Um palpite de jogo finalizado (lista filtrável).
public class StatBetItemViewModel
{
    public Guid MatchId { get; set; }
    public string MatchLabel { get; set; } = string.Empty;
    public DateTime MatchDate { get; set; } // BRT
    public string ResultLabel { get; set; } = string.Empty; // "0x0"
    public string UserName { get; set; } = string.Empty;
    public string BetLabel { get; set; } = string.Empty;     // "1x0"
    public int Points { get; set; }
    public ScoreType AppliedRule { get; set; }
}

// Destaque de jogo (mini-ranking). Count para "mais cravadas"; Average para média.
public class GameHighlightViewModel
{
    public Guid MatchId { get; set; }
    public string MatchLabel { get; set; } = string.Empty;
    public string ResultLabel { get; set; } = string.Empty;
    public int Count { get; set; }
    public double Average { get; set; }
}

// Linha de consenso por jogo.
public class ConsensusRowViewModel
{
    public Guid MatchId { get; set; }
    public string MatchLabel { get; set; } = string.Empty;
    public DateTime MatchDate { get; set; } // BRT
    public string ResultLabel { get; set; } = string.Empty;
    public string ConsensusLabel { get; set; } = string.Empty; // placar mais palpitado
    public int ConsensusCount { get; set; }
    public int ExactCount { get; set; }                        // quantos cravaram
    public string TopThree { get; set; } = string.Empty;       // "2x1 (4), 1x0 (3), 0x0 (2)"
}
