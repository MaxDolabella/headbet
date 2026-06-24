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

    /// <summary>Jogos em que só um cravou o placar exato e todos os outros erraram o resultado.</summary>
    public List<SoloMatchViewModel> SoloHit { get; set; } = [];

    /// <summary>Jogos em que todos cravaram o placar exato e só um errou o resultado (o contrário).</summary>
    public List<SoloMatchViewModel> SoloMiss { get; set; } = [];

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

// Grupo de consenso por valor de repetição: jogos cujo placar mais repetido teve
// exatamente RepeatCount palpites iguais (independente de qual placar foi).
public class ConsensusGroupViewModel
{
    public int RepeatCount { get; set; }  // nº de palpites iguais no placar mais repetido
    public int GameCount { get; set; }    // em quantos jogos esse valor apareceu
    public List<ConsensusGameViewModel> Games { get; set; } = [];
}

// Jogo "lobo solitário": um participante isolado (cravou sozinho, ou errou sozinho).
public class SoloMatchViewModel
{
    public Guid MatchId { get; set; }
    public string MatchLabel { get; set; } = string.Empty;
    public DateTime MatchDate { get; set; } // BRT
    public string ResultLabel { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;  // o solitário
    public string BetLabel { get; set; } = string.Empty;  // palpite do solitário
    public int OthersCount { get; set; }                  // quantos eram os "outros"
}

// Um jogo dentro de um grupo de consenso.
public class ConsensusGameViewModel
{
    public Guid MatchId { get; set; }
    public string MatchLabel { get; set; } = string.Empty;
    public DateTime MatchDate { get; set; } // BRT
    public string ResultLabel { get; set; } = string.Empty;
    public string ConsensusLabel { get; set; } = string.Empty; // placar mais repetido ("3x0")
    public int ExactCount { get; set; }                        // quantos cravaram o resultado real
}
