using Headsoft.Messaging.Abstractions.Queries;
using HeadBet.Core.Domain.Interfaces.Repositories;
using HeadBet.Core.Models;

namespace HeadBet.Core.Application.Queries;

// --- Query ---
public record GetAiModelQuery(Guid Id) : QueryBase<AiModelFormViewModel?>;

// --- Handler ---
public sealed class GetAiModelQueryHandler(
    IAiModelRepository repository) : QueryHandlerBase<GetAiModelQuery, AiModelFormViewModel?>
{
    public override async Task<AiModelFormViewModel?> HandleAsync(GetAiModelQuery query, CancellationToken ct)
    {
        return await repository.GetByIdAsync<AiModelFormViewModel>([query.Id], ct);
    }
}
