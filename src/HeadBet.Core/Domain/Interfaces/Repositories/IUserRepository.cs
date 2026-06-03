using Headsoft.Core.Interfaces.Repositories;
using HeadBet.Core.Domain.Entities;

namespace HeadBet.Core.Domain.Interfaces.Repositories;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email, bool @readonly, CancellationToken ct);
    Task<bool> EmailExistsAsync(string email, Guid? excludeId, CancellationToken ct);
}
