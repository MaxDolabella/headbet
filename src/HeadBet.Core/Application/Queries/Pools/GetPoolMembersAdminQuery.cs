using Headsoft.Core.Sorting;
using Headsoft.Messaging.Abstractions.Queries;
using HeadBet.Core.Domain.Enums;
using HeadBet.Core.Domain.Interfaces;
using HeadBet.Core.Domain.Interfaces.Repositories;
using HeadBet.Core.Extensions;
using HeadBet.Core.Models;

namespace HeadBet.Core.Application.Queries;

// --- Query ---
public record GetPoolMembersAdminQuery(Guid PoolId) : QueryBase<PoolMembersAdminViewModel?>;

// --- Handler ---
public sealed class GetPoolMembersAdminQueryHandler(
    IPoolRepository poolRepository,
    IPoolMemberRepository memberRepository,
    IUserContext userContext)
    : QueryHandlerBase<GetPoolMembersAdminQuery, PoolMembersAdminViewModel?>
{
    public override async Task<PoolMembersAdminViewModel?> HandleAsync(GetPoolMembersAdminQuery query, CancellationToken ct)
    {
        var userId = userContext.UserId;

        var isAdmin = await memberRepository.AnyAsync(
            m => m.PoolId == query.PoolId
                 && m.UserId == userId
                 && m.Role == PoolMemberRole.Admin
                 && m.Status == PoolMemberStatus.Active,
            ct);

        if (!isAdmin)
            return null;

        var pool = await poolRepository.GetAsync(p => p.Id == query.PoolId, @readonly: true, ct);
        if (pool is null)
            return null;

        var members = await memberRepository.ToListAsync<PoolMemberItemViewModel>(
            m => m.PoolId == query.PoolId, null, m => m.JoinedAt, SortDirection.Ascending, ct);

        foreach (var m in members)
        {
            m.JoinedAt = m.JoinedAt.ToBrt();
            m.IsCurrentUser = m.UserId == userId;
        }

        return new PoolMembersAdminViewModel
        {
            PoolId = pool.Id,
            PoolName = pool.Name,
            Members = members,
        };
    }
}
