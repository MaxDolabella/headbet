using Headsoft.Core.Sorting;
using Headsoft.Messaging.Abstractions.Queries;
using HeadBet.Core.Domain.Enums;
using HeadBet.Core.Domain.Interfaces;
using HeadBet.Core.Domain.Interfaces.Repositories;
using HeadBet.Core.Extensions;
using HeadBet.Core.Models;

namespace HeadBet.Core.Application.Queries;

// --- Query ---
public record GetMatchQuery(Guid Id, Guid PoolId) : QueryBase<MatchFormViewModel?>;

// --- Handler ---
public sealed class GetMatchQueryHandler(
    IMatchRepository matchRepository,
    ITeamRepository teamRepository,
    IPoolMemberRepository memberRepository,
    IUserContext userContext) : QueryHandlerBase<GetMatchQuery, MatchFormViewModel?>
{
    public override async Task<MatchFormViewModel?> HandleAsync(GetMatchQuery query, CancellationToken ct)
    {
        var isMember = await memberRepository.AnyAsync(
            m => m.PoolId == query.PoolId
                 && m.UserId == userContext.UserId
                 && m.Status == PoolMemberStatus.Active,
            ct);

        if (!isMember)
            return null;

        var vm = await matchRepository.GetAsync<MatchFormViewModel>(
            m => m.Id == query.Id && m.PoolId == query.PoolId, ct);

        if (vm is null)
            return null;

        vm.MatchDate = vm.MatchDate.ToBrt();

        vm.AvailableTeams = await teamRepository.ToListAsync<TeamListViewModel>(
            t => t.PoolId == query.PoolId,
            null,
            t => t.Name,
            SortDirection.Ascending,
            ct);

        return vm;
    }
}
