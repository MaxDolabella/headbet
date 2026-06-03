using AutoMapper;
using Headsoft.Core.Data;
using HeadBet.Core.Domain.Entities;
using HeadBet.Core.Domain.Interfaces.Repositories;

namespace HeadBet.Core.Infrastructure.Data.Repositories;

public sealed class PasswordResetTokenRepository(AppDbContext context, IMapper mapper)
    : RepositoryBase<PasswordResetToken>(context, mapper), IPasswordResetTokenRepository
{
    public async Task<PasswordResetToken?> GetByTokenAsync(string token, bool @readonly, CancellationToken ct)
        => await GetAsync(t => t.Token == token, @readonly, ct);
}
