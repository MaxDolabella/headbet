using AutoMapper;
using Headsoft.Core.Data;
using HeadBet.Core.Domain.Entities;
using HeadBet.Core.Domain.Interfaces.Repositories;

namespace HeadBet.Core.Infrastructure.Data.Repositories;

public sealed class UserRepository(AppDbContext context, IMapper mapper)
    : RepositoryBase<User>(context, mapper), IUserRepository
{
    public async Task<User?> GetByEmailAsync(string email, bool @readonly, CancellationToken ct)
    {
        var normalized = email.Trim();
        return await GetAsync(u => u.Email == normalized, @readonly, ct);
    }

    public async Task<bool> EmailExistsAsync(string email, Guid? excludeId, CancellationToken ct)
    {
        var normalized = email.Trim();
        return await AnyAsync(
            u => u.Email == normalized && (!excludeId.HasValue || u.Id != excludeId.Value),
            ct);
    }
}
