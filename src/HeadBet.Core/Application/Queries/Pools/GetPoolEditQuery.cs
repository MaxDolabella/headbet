using AutoMapper;
using Headsoft.Core.Sorting;
using Headsoft.Messaging.Abstractions.Queries;
using HeadBet.Core.Domain.Enums;
using HeadBet.Core.Domain.Interfaces;
using HeadBet.Core.Domain.Interfaces.Repositories;
using HeadBet.Core.Models;

namespace HeadBet.Core.Application.Queries;

// --- Query ---
public record GetPoolEditQuery(Guid Id) : QueryBase<PoolEditViewModel?>;

// --- Handler ---
public sealed class GetPoolEditQueryHandler(
    IPoolRepository poolRepository,
    IPoolMemberRepository memberRepository,
    IPoolPrizeRepository prizeRepository,
    IMapper mapper,
    IUserContext userContext)
    : QueryHandlerBase<GetPoolEditQuery, PoolEditViewModel?>
{
    public override async Task<PoolEditViewModel?> HandleAsync(GetPoolEditQuery query, CancellationToken ct)
    {
        var userId = userContext.UserId;

        var isAdmin = await memberRepository.AnyAsync(
            m => m.PoolId == query.Id
                 && m.UserId == userId
                 && m.Role == PoolMemberRole.Admin
                 && m.Status == PoolMemberStatus.Active,
            ct);

        if (!isAdmin)
            return null;

        var pool = await poolRepository.GetAsync(p => p.Id == query.Id, @readonly: true, ct);
        if (pool is null)
            return null;

        var vm = mapper.Map<PoolEditViewModel>(pool);

        vm.Prizes = await prizeRepository.ToListAsync<PrizeItemViewModel>(
            p => p.PoolId == query.Id, null, p => p.Position, SortDirection.Ascending, ct);

        // Valor arrecadado: usa o persistido se houver; senão estima (taxa × participantes ativos) como ponto de partida.
        if (pool.CollectedAmount is null)
        {
            var activeMemberCount = await memberRepository.CountAsync(
                m => m.PoolId == query.Id && m.Status == PoolMemberStatus.Active, ct);
            vm.CollectedAmount = (pool.EntryFee ?? 0m) * activeMemberCount;
        }

        return vm;
    }
}
