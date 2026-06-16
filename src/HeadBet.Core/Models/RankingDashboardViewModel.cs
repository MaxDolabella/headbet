using HeadBet.Core.Domain.Enums;

namespace HeadBet.Core.Models;

public class RankingDashboardViewModel
{
    public bool HasData { get; set; }

    /// <summary>Um rótulo por jogo finalizado (ordem cronológica) — eixo X dos gráficos de linha.</summary>
    public List<string> MatchLabels { get; set; } = [];

    /// <summary>Participantes ordenados pela posição final.</summary>
    public List<RankingDashboardParticipant> Participants { get; set; } = [];
}

public class RankingDashboardParticipant
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public bool IsCurrentUser { get; set; }
    public int FinalPosition { get; set; }

    /// <summary>Pontos acumulados após cada jogo (mesmo comprimento de MatchLabels).</summary>
    public double[] CumulativePoints { get; set; } = [];

    /// <summary>Posição no ranking após cada jogo (1 = líder).</summary>
    public double[] Positions { get; set; } = [];

    public Dictionary<ScoreType, int> CountsByScoreType { get; set; } = new();

    /// <summary>Pontos obtidos ÷ máximo possível (nº de jogos × pontos do placar exato), em %.</summary>
    public double EfficiencyPercent { get; set; }
}
