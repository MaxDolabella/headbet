using HeadBet.Core.Application.DTOs;

namespace HeadBet.Core.Domain.Interfaces;

public interface IFootballDataClient
{
    Task<FootballDataCompetitionsResponse> GetCompetitionsAsync(CancellationToken ct);
    Task<FootballDataTeamsResponse> GetTeamsAsync(int competitionId, CancellationToken ct);
    Task<FootballDataMatchesResponse> GetMatchesAsync(int competitionId, CancellationToken ct);
}
