namespace HeadBet.Core.Models;

public class TeamListViewModel
{
    public Guid Id { get; set; }
    public Guid PoolId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Abbreviation { get; set; } = string.Empty;
    public string? FlagUrl { get; set; }
}
