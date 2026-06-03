using HeadBet.Core.Domain.Enums;

namespace HeadBet.Core.Models;

public class PoolDetailsViewModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsPaid { get; set; }
    public decimal? EntryFee { get; set; }
    public decimal CollectedAmount { get; set; }
    public bool AutoAccept { get; set; }
    public bool IsActive { get; set; }
    public bool IsPublic { get; set; }
    public PrizeMode PrizeMode { get; set; }
    public DateTime CreatedAt { get; set; }
    public string InviteCode { get; set; } = string.Empty;

    public PoolMemberRole? MyRole { get; set; }
    public bool IsMember => MyRole.HasValue;
    public bool IsAdmin => MyRole == PoolMemberRole.Admin;

    public List<ScoringRuleItemViewModel> ScoringRules { get; set; } = [];
    public List<PrizeItemViewModel> Prizes { get; set; } = [];
    public List<PoolMemberItemViewModel> Members { get; set; } = [];
    public List<MatchListViewModel> UpcomingMatches { get; set; } = [];
    public List<MatchListViewModel> FinishedMatches { get; set; } = [];
    public List<RankingItemViewModel> Ranking { get; set; } = [];
}
