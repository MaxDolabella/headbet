using HeadBet.Core.Application.DTOs;

namespace HeadBet.Core.Infrastructure.Tournament;

public interface ITournamentImporter
{
    Task<TournamentSetupResult> FetchPreviewAsync(int competitionId, Guid poolId, CancellationToken ct);

    Task ImportAsync(TournamentSetupResult data, Guid poolId, CancellationToken ct);
}
