using Headsoft.Messaging.Abstractions.Queries;
using HeadBet.Core.Domain.Enums;
using HeadBet.Core.Domain.Interfaces;
using HeadBet.Core.Domain.Interfaces.Repositories;
using HeadBet.Core.Models;

namespace HeadBet.Core.Application.Queries;

// --- Query ---
public record GetPoolAdminNavQuery(Guid PoolId) : QueryBase<PoolAdminNavViewModel>;

// --- Handler ---
public sealed class GetPoolAdminNavQueryHandler(
    IPoolMemberRepository memberRepository,
    IUserContext userContext)
    : QueryHandlerBase<GetPoolAdminNavQuery, PoolAdminNavViewModel>
{
    public override async Task<PoolAdminNavViewModel> HandleAsync(GetPoolAdminNavQuery query, CancellationToken ct)
    {
        var userId = userContext.UserId;

        var isAdmin = await memberRepository.AnyAsync(
            m => m.PoolId == query.PoolId
                 && m.UserId == userId
                 && m.Role == PoolMemberRole.Admin
                 && m.Status == PoolMemberStatus.Active,
            ct);

        if (!isAdmin)
            return new PoolAdminNavViewModel();

        var pending = await memberRepository.CountAsync(
            m => m.PoolId == query.PoolId && m.Status == PoolMemberStatus.Pending, ct);

        return new PoolAdminNavViewModel { IsAdmin = true, PendingCount = (int)pending };
    }
}
