using Headsoft.Messaging.Abstractions.Queries;
using HeadBet.Core.Domain.Enums;
using HeadBet.Core.Domain.Interfaces;
using HeadBet.Core.Domain.Interfaces.Repositories;
using HeadBet.Core.Models;

namespace HeadBet.Core.Application.Queries;

// --- Query ---
public record GetSetupWizardCompetitionsQuery(Guid PoolId) : QueryBase<SetupWizardCompetitionListViewModel?>;

// --- Handler ---
public sealed class GetSetupWizardCompetitionsQueryHandler(
    IPoolRepository poolRepository,
    IPoolMemberRepository memberRepository,
    IFootballDataClient footballDataClient,
    IUserContext userContext)
    : QueryHandlerBase<GetSetupWizardCompetitionsQuery, SetupWizardCompetitionListViewModel?>
{
    public override async Task<SetupWizardCompetitionListViewModel?> HandleAsync(
        GetSetupWizardCompetitionsQuery query, CancellationToken ct)
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

        var response = await footballDataClient.GetCompetitionsAsync(ct);

        var groups = response.Competitions
            .GroupBy(c => new
            {
                AreaName = c.Area?.Name ?? "Outros",
                AreaFlag = c.Area?.Flag,
            })
            .OrderBy(g => g.Key.AreaName)
            .Select(g => new CompetitionGroupViewModel
            {
                AreaName = g.Key.AreaName,
                AreaFlagUrl = g.Key.AreaFlag,
                Competitions = g
                    .OrderBy(c => c.Name)
                    .Select(c => new CompetitionItemViewModel
                    {
                        Id = c.Id,
                        Name = c.Name,
                        Code = c.Code,
                        EmblemUrl = c.Emblem,
                    })
                    .ToList(),
            })
            .ToList();

        return new SetupWizardCompetitionListViewModel
        {
            PoolId = pool.Id,
            PoolName = pool.Name,
            CompetitionsByArea = groups,
        };
    }
}
