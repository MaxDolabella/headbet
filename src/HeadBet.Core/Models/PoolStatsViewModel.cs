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
    public List<ConsensusGroupViewModel> Consensus { get; set; } = [];

    /// <summary>Participantes distintos com palpite (para o filtro da lista de palpites).</summary>
    public List<string> Users { get; set; } = [];
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

// Grupo de consenso: um placar que foi o mais palpitado em um ou mais jogos.
public class ConsensusGroupViewModel
{
    public string ConsensusLabel { get; set; } = string.Empty; // placar "1x0"
    public int GameCount { get; set; }                         // em quantos jogos foi o consenso
    public int TotalVotes { get; set; }                        // soma dos votos nesse placar
    public List<ConsensusGameViewModel> Games { get; set; } = [];
}

// Um jogo dentro de um grupo de consenso.
public class ConsensusGameViewModel
{
    public Guid MatchId { get; set; }
    public string MatchLabel { get; set; } = string.Empty;
    public DateTime MatchDate { get; set; } // BRT
    public string ResultLabel { get; set; } = string.Empty;
    public int Count { get; set; }      // quantos palpitaram o placar do consenso neste jogo
    public int ExactCount { get; set; } // quantos cravaram o resultado real
}
