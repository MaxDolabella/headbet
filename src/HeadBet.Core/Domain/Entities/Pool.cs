using Headsoft.Core.Entities;
using HeadBet.Core.Domain.Enums;

namespace HeadBet.Core.Domain.Entities;

public class Pool : Entity<Guid>
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsPaid { get; set; }
    public decimal? EntryFee { get; set; }

    /// <summary>
    /// Valor efetivamente arrecadado, editável pelo admin. Quando null, a distribuição
    /// percentual cai de volta para a estimativa (taxa × participantes ativos).
    /// </summary>
    public decimal? CollectedAmount { get; set; }
    public bool AutoAccept { get; set; }
    public bool IsActive { get; set; }
    public bool IsPublic { get; set; }
    public PrizeMode PrizeMode { get; set; } = PrizeMode.Percentage;
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Código opaco usado no link de convite (/join/{code}). Não é o Id do bolão,
    /// para que o link não seja forjável a partir do Id.
    /// </summary>
    public string InviteCode { get; set; } = string.Empty;
}
