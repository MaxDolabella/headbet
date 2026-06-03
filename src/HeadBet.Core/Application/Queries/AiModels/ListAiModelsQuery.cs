using System.Linq.Expressions;
using Headsoft.Messaging.Abstractions.Queries;
using HeadBet.Core.Domain.Entities;
using HeadBet.Core.Domain.Interfaces.Repositories;
using HeadBet.Core.Models;

namespace HeadBet.Core.Application.Queries;

// --- Query ---
public record ListAiModelsQuery : QueryBase<List<AiModelListViewModel>>;

// --- Handler ---
public sealed class ListAiModelsQueryHandler(
    IAiModelRepository repository) : QueryHandlerBase<ListAiModelsQuery, List<AiModelListViewModel>>
{
    public override async Task<List<AiModelListViewModel>> HandleAsync(ListAiModelsQuery query, CancellationToken ct)
    {
        return await repository.ToListAsync<AiModelListViewModel>((Expression<Func<AiModel, bool>>?)null, ct);
    }
}
