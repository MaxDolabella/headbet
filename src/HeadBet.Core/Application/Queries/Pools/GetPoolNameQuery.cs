using Headsoft.Messaging.Abstractions.Queries;
using HeadBet.Core.Domain.Interfaces.Repositories;

namespace HeadBet.Core.Application.Queries;

// --- Query ---
public record GetPoolNameQuery(Guid PoolId) : QueryBase<string?>;

// --- Handler ---
public sealed class GetPoolNameQueryHandler(IPoolRepository poolRepository)
    : QueryHandlerBase<GetPoolNameQuery, string?>
{
    public override async Task<string?> HandleAsync(GetPoolNameQuery query, CancellationToken ct)
    {
        var pool = await poolRepository.GetAsync(p => p.Id == query.PoolId, @readonly: true, ct);
        return pool?.Name;
    }
}
