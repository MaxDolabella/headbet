using Headsoft.Messaging.Abstractions.Queries;
using HeadBet.Core.Domain.Interfaces;
using HeadBet.Core.Domain.Interfaces.Repositories;
using HeadBet.Core.Models;

namespace HeadBet.Core.Application.Queries;

// --- Query ---
public record ListPoolsQuery : QueryBase<PoolListingViewModel>;

// --- Handler ---
public sealed class ListPoolsQueryHandler(
    IPoolRepository poolRepository,
    IUserContext userContext)
    : QueryHandlerBase<ListPoolsQuery, PoolListingViewModel>
{
    public override async Task<PoolListingViewModel> HandleAsync(ListPoolsQuery query, CancellationToken ct)
    {
        var userId = userContext.UserId;

        var mine = await poolRepository.ListByUserAsync(userId, ct);
        var publicAvailable = await poolRepository.ListPublicAvailableAsync(userId, ct);

        return new PoolListingViewModel
        {
            Mine = mine,
            Public = publicAvailable,
        };
    }
}
