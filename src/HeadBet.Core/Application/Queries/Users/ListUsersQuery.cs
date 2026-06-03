using System.Linq.Expressions;
using Headsoft.Messaging.Abstractions.Queries;
using HeadBet.Core.Domain.Entities;
using HeadBet.Core.Domain.Interfaces.Repositories;
using HeadBet.Core.Models;

namespace HeadBet.Core.Application.Queries;

// --- Query ---
public record ListUsersQuery : QueryBase<List<UserListViewModel>>;

// --- Handler ---
public sealed class ListUsersQueryHandler(
    IUserRepository repository) : QueryHandlerBase<ListUsersQuery, List<UserListViewModel>>
{
    public override async Task<List<UserListViewModel>> HandleAsync(ListUsersQuery query, CancellationToken ct)
    {
        return await repository.ToListAsync<UserListViewModel>((Expression<Func<User, bool>>?)null, ct);
    }
}
