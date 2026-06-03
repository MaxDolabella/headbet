namespace HeadBet.Core.Application.DTOs;

public class MatchKeyDto
{
    public Guid HomeTeamId { get; set; }
    public Guid AwayTeamId { get; set; }
    public DateTime MatchDate { get; set; }
}
