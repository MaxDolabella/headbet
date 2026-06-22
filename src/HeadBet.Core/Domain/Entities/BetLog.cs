using Headsoft.Core.Entities;
using HeadBet.Core.Domain.Enums;

namespace HeadBet.Core.Domain.Entities;

/// <summary>
/// Registro de auditoria (append-only) de criação/atualização de palpite.
/// Sem navegações: não deve ser arrastado por cascade nem depender das linhas relacionadas.
/// </summary>
public class BetLog : Entity<Guid>
{
    public Guid MatchId { get; set; }
    public Guid UserId { get; set; }
    public Guid PoolId { get; set; }
    public BetLogAction Action { get; set; }
    public int? OldHomeScore { get; set; }
    public int? OldAwayScore { get; set; }
    public int NewHomeScore { get; set; }
    public int NewAwayScore { get; set; }
    public DateTime CreatedAt { get; set; }
}
