using Headsoft.Messaging.Abstractions.Queries;
using HeadBet.Core.Domain.Interfaces;
using HeadBet.Core.Domain.Interfaces.Repositories;
using HeadBet.Core.Extensions;
using HeadBet.Core.Models;

namespace HeadBet.Core.Application.Queries;

// --- Query ---
public record ListUserApiKeysQuery : QueryBase<List<UserApiKeyListViewModel>>;

// --- Handler ---
public sealed class ListUserApiKeysQueryHandler(
    IUserApiKeyRepository repository,
    IUserContext userContext) : QueryHandlerBase<ListUserApiKeysQuery, List<UserApiKeyListViewModel>>
{
    public override async Task<List<UserApiKeyListViewModel>> HandleAsync(ListUserApiKeysQuery query, CancellationToken ct)
    {
        var keys = await repository.ToListAsync<UserApiKeyListViewModel>(
            k => k.UserId == userContext.UserId, ct);

        foreach (var k in keys)
            k.CreatedAt = k.CreatedAt.ToBrt();

        return keys;
    }
}
