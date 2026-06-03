using Headsoft.Core.Entities;

namespace HeadBet.Core.Domain.Entities;

public class Team : Entity<Guid>
{
    public Guid PoolId { get; set; }
    public int? ExternalId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Abbreviation { get; set; } = string.Empty;
    public string? FlagUrl { get; set; }

    public Pool Pool { get; set; } = null!;
}
