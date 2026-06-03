using Headsoft.Messaging.Abstractions.Queries;
using HeadBet.Core.Domain.Enums;
using HeadBet.Core.Domain.Interfaces;
using HeadBet.Core.Domain.Interfaces.Repositories;
using HeadBet.Core.Models;

namespace HeadBet.Core.Application.Queries;

// --- Query ---
public record GetTeamQuery(Guid Id, Guid PoolId) : QueryBase<TeamFormViewModel?>;

// --- Handler ---
public sealed class GetTeamQueryHandler(
    ITeamRepository teamRepository,
    IPoolMemberRepository memberRepository,
    IUserContext userContext) : QueryHandlerBase<GetTeamQuery, TeamFormViewModel?>
{
    public override async Task<TeamFormViewModel?> HandleAsync(GetTeamQuery query, CancellationToken ct)
    {
        var isMember = await memberRepository.AnyAsync(
            m => m.PoolId == query.PoolId
                 && m.UserId == userContext.UserId
                 && m.Status == PoolMemberStatus.Active,
            ct);

        if (!isMember)
            return null;

        return await teamRepository.GetAsync<TeamFormViewModel>(
            t => t.Id == query.Id && t.PoolId == query.PoolId, ct);
    }
}
