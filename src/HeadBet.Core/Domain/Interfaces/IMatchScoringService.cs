namespace HeadBet.Core.Domain.Interfaces;

public interface IMatchScoringService
{
    Task RecomputeForMatchAsync(Guid matchId, CancellationToken ct);
    Task RecomputeForUserBetAsync(Guid matchId, Guid userId, CancellationToken ct);
}
