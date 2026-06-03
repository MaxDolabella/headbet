using Headsoft.Messaging.Abstractions.Queries;
using HeadBet.Core.Domain.Enums;
using HeadBet.Core.Domain.Interfaces;
using HeadBet.Core.Domain.Interfaces.Repositories;
using HeadBet.Core.Extensions;
using HeadBet.Core.Infrastructure.Tournament;
using HeadBet.Core.Models;

namespace HeadBet.Core.Application.Queries;

// --- Query ---
public record GetSetupWizardPreviewQuery(Guid PoolId, int CompetitionId) : QueryBase<SetupWizardPreviewViewModel?>;

// --- Handler ---
public sealed class GetSetupWizardPreviewQueryHandler(
    IPoolRepository poolRepository,
    IPoolMemberRepository memberRepository,
    IFootballDataClient footballDataClient,
    ITournamentImporter tournamentImporter,
    IUserContext userContext)
    : QueryHandlerBase<GetSetupWizardPreviewQuery, SetupWizardPreviewViewModel?>
{
    public override async Task<SetupWizardPreviewViewModel?> HandleAsync(
        GetSetupWizardPreviewQuery query, CancellationToken ct)
    {
        var pool = await poolRepository.GetAsync(p => p.Id == query.PoolId, @readonly: true, ct);
        if (pool is null)
            return null;

        var isAdmin = await memberRepository.AnyAsync(
            m => m.PoolId == query.PoolId
                 && m.UserId == userContext.UserId
                 && m.Role == PoolMemberRole.Admin
                 && m.Status == PoolMemberStatus.Active,
            ct);

        if (!isAdmin)
            return null;

        var competitionsTask = footballDataClient.GetCompetitionsAsync(ct);
        var previewTask = tournamentImporter.FetchPreviewAsync(query.CompetitionId, query.PoolId, ct);
        await Task.WhenAll(competitionsTask, previewTask);

        var competition = competitionsTask.Result.Competitions
            .FirstOrDefault(c => c.Id == query.CompetitionId);

        if (competition is null)
            return null;

        var data = previewTask.Result;

        return new SetupWizardPreviewViewModel
        {
            PoolId = pool.Id,
            PoolName = pool.Name,
            CompetitionId = competition.Id,
            CompetitionName = competition.Name,
            CompetitionEmblemUrl = competition.Emblem,
            AreaName = competition.Area?.Name,
            TeamCount = data.Teams.Count,
            MatchCount = data.Matches.Count,
            FirstMatchDateBrt = data.Matches.Count > 0 ? data.Matches.Min(m => m.MatchDateUtc).ToBrt() : null,
            LastMatchDateBrt = data.Matches.Count > 0 ? data.Matches.Max(m => m.MatchDateUtc).ToBrt() : null,
            Teams = data.Teams
                .OrderBy(t => t.Name)
                .Select(t => new TeamPreviewRow
                {
                    Name = t.Name,
                    Abbreviation = t.Abbreviation,
                    FlagUrl = t.FlagUrl,
                })
                .ToList(),
            Matches = data.Matches
                .OrderBy(m => m.MatchDateUtc)
                .Select(m => new MatchPreviewRow
                {
                    MatchDateBrt = m.MatchDateUtc.ToBrt(),
                    HomeTeamAbbreviation = m.HomeTeamAbbreviation,
                    AwayTeamAbbreviation = m.AwayTeamAbbreviation,
                    Status = m.Status,
                    HomeScore = m.HomeScore,
                    AwayScore = m.AwayScore,
                    Group = m.Group,
                    Round = m.Round,
                })
                .ToList(),
        };
    }
}
