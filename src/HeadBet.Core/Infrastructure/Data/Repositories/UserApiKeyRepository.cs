using AutoMapper;
using Headsoft.Core.Data;
using HeadBet.Core.Domain.Entities;
using HeadBet.Core.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace HeadBet.Core.Infrastructure.Data.Repositories;

public sealed class UserApiKeyRepository(AppDbContext context, IMapper mapper)
    : RepositoryBase<UserApiKey>(context, mapper), IUserApiKeyRepository
{
    public async Task ClearDefaultAsync(Guid userId, Guid? excludeId, CancellationToken ct)
    {
        var defaultKeys = await DbSet
            .AsTracking()
            .Where(k => k.UserId == userId && k.IsDefault && k.Id != excludeId)
            .ToListAsync(ct);

        foreach (var key in defaultKeys)
            key.IsDefault = false;
    }
}
