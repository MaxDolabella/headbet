using Headsoft.Core.Interfaces.Repositories;
using HeadBet.Core.Domain.Entities;

namespace HeadBet.Core.Domain.Interfaces.Repositories;

public interface IPasswordResetTokenRepository : IRepository<PasswordResetToken>
{
    Task<PasswordResetToken?> GetByTokenAsync(string token, bool @readonly, CancellationToken ct);
}
