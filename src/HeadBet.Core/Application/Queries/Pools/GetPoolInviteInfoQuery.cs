using Headsoft.Messaging.Abstractions.Queries;
using HeadBet.Core.Domain.Enums;
using HeadBet.Core.Domain.Interfaces;
using HeadBet.Core.Domain.Interfaces.Repositories;
using HeadBet.Core.Models;

namespace HeadBet.Core.Application.Queries;

// --- Query ---
// Resolve o convite pelo código opaco do link (/join/{code}), nunca pelo Id do bolão.
public record GetPoolInviteInfoQuery(string Code) : QueryBase<PoolInviteViewModel?>;

// --- Handler ---
public sealed class GetPoolInviteInfoQueryHandler(
    IPoolRepository poolRepository,
    IPoolMemberRepository memberRepository,
    IUserContext userContext)
    : QueryHandlerBase<GetPoolInviteInfoQuery, PoolInviteViewModel?>
{
    public override async Task<PoolInviteViewModel?> HandleAsync(GetPoolInviteInfoQuery query, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(query.Code))
            return null;

        var userId = userContext.UserId;

        var pool = await poolRepository.GetAsync(p => p.InviteCode == query.Code, @readonly: true, ct);
        if (pool is null)
            return null;

        var me = await memberRepository.GetAsync(m => m.PoolId == pool.Id && m.UserId == userId, @readonly: true, ct);

        var memberCount = await memberRepository.CountAsync(
            m => m.PoolId == pool.Id && m.Status == PoolMemberStatus.Active, ct);

        return new PoolInviteViewModel
        {
            Id = pool.Id,
            InviteCode = pool.InviteCode,
            Name = pool.Name,
            Description = pool.Description,
            IsPaid = pool.IsPaid,
            EntryFee = pool.EntryFee,
            IsActive = pool.IsActive,
            IsPublic = pool.IsPublic,
            MemberCount = memberCount,
            MyStatus = me?.Status,
        };
    }
}
