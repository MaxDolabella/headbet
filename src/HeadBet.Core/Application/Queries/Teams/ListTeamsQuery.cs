using Headsoft.Core.Sorting;
using Headsoft.Messaging.Abstractions.Queries;
using HeadBet.Core.Domain.Enums;
using HeadBet.Core.Domain.Interfaces;
using HeadBet.Core.Domain.Interfaces.Repositories;
using HeadBet.Core.Models;

namespace HeadBet.Core.Application.Queries;

// --- Query ---
public record ListTeamsQuery(Guid PoolId) : QueryBase<List<TeamListViewModel>>;

// --- Handler ---
public sealed class ListTeamsQueryHandler(
    ITeamRepository teamRepository,
    IPoolMemberRepository memberRepository,
    IUserContext userContext) : QueryHandlerBase<ListTeamsQuery, List<TeamListViewModel>>
{
    public override async Task<List<TeamListViewModel>> HandleAsync(ListTeamsQuery query, CancellationToken ct)
    {
        var isMember = await memberRepository.AnyAsync(
            m => m.PoolId == query.PoolId
                 && m.UserId == userContext.UserId
                 && m.Status == PoolMemberStatus.Active,
            ct);

        if (!isMember)
            return [];

        return await teamRepository.ToListAsync<TeamListViewModel>(
            t => t.PoolId == query.PoolId,
            null,
            t => t.Name,
            SortDirection.Ascending,
            ct);
    }
}
