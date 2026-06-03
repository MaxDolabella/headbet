using System.Net.Http.Json;
using HeadBet.Core.Application.DTOs;
using HeadBet.Core.Domain.Interfaces;

namespace HeadBet.Core.Infrastructure.Http;

public sealed class FootballDataClient(HttpClient httpClient) : IFootballDataClient
{
    public async Task<FootballDataCompetitionsResponse> GetCompetitionsAsync(CancellationToken ct)
    {
        var response = await httpClient.GetFromJsonAsync<FootballDataCompetitionsResponse>(
            "v4/competitions", ct);

        return response ?? new FootballDataCompetitionsResponse();
    }

    public async Task<FootballDataTeamsResponse> GetTeamsAsync(int competitionId, CancellationToken ct)
    {
        var response = await httpClient.GetFromJsonAsync<FootballDataTeamsResponse>(
            $"v4/competitions/{competitionId}/teams", ct);

        return response ?? new FootballDataTeamsResponse();
    }

    public async Task<FootballDataMatchesResponse> GetMatchesAsync(int competitionId, CancellationToken ct)
    {
        var response = await httpClient.GetFromJsonAsync<FootballDataMatchesResponse>(
            $"v4/competitions/{competitionId}/matches", ct);

        return response ?? new FootballDataMatchesResponse();
    }
}
