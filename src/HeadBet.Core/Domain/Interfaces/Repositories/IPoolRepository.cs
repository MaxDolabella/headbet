using Headsoft.Core.Interfaces.Repositories;
using HeadBet.Core.Domain.Entities;
using HeadBet.Core.Models;

namespace HeadBet.Core.Domain.Interfaces.Repositories;

public interface IPoolRepository : IRepository<Pool>
{
    Task<List<PoolListViewModel>> ListByUserAsync(Guid userId, CancellationToken ct);
    Task<List<PoolListViewModel>> ListPublicAvailableAsync(Guid userId, CancellationToken ct);
}
