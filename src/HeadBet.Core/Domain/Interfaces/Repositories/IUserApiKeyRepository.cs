using Headsoft.Core.Interfaces.Repositories;
using HeadBet.Core.Domain.Entities;

namespace HeadBet.Core.Domain.Interfaces.Repositories;

public interface IUserApiKeyRepository : IRepository<UserApiKey>
{
    Task ClearDefaultAsync(Guid userId, Guid? excludeId, CancellationToken ct);
}
