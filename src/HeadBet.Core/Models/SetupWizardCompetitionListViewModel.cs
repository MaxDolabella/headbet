namespace HeadBet.Core.Models;

public class SetupWizardCompetitionListViewModel
{
    public Guid PoolId { get; set; }
    public string PoolName { get; set; } = string.Empty;
    public List<CompetitionGroupViewModel> CompetitionsByArea { get; set; } = [];
}

public class CompetitionGroupViewModel
{
    public string AreaName { get; set; } = string.Empty;
    public string? AreaFlagUrl { get; set; }
    public List<CompetitionItemViewModel> Competitions { get; set; } = [];
}

public class CompetitionItemViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string? EmblemUrl { get; set; }
}
