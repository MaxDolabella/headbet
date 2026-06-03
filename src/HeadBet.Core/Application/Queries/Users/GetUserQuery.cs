using Headsoft.Messaging.Abstractions.Queries;
using HeadBet.Core.Domain.Interfaces.Repositories;
using HeadBet.Core.Models;

namespace HeadBet.Core.Application.Queries;

// --- Query ---
public record GetUserQuery(Guid Id) : QueryBase<UserFormViewModel?>;

// --- Handler ---
public sealed class GetUserQueryHandler(
    IUserRepository repository) : QueryHandlerBase<GetUserQuery, UserFormViewModel?>
{
    public override async Task<UserFormViewModel?> HandleAsync(GetUserQuery query, CancellationToken ct)
    {
        return await repository.GetByIdAsync<UserFormViewModel>([query.Id], ct);
    }
}
