using Headsoft.Messaging.Abstractions.Queries;
using HeadBet.Core.Domain.Interfaces;
using HeadBet.Core.Domain.Interfaces.Repositories;
using HeadBet.Core.Models;

namespace HeadBet.Core.Application.Queries;

// --- Query ---
/// <summary>Retorna o perfil do usuário autenticado (resolvido via IUserContext).</summary>
public record GetProfileQuery : QueryBase<ProfileViewModel?>;

// --- Handler ---
public sealed class GetProfileQueryHandler(
    IUserRepository repository,
    IUserContext userContext) : QueryHandlerBase<GetProfileQuery, ProfileViewModel?>
{
    public override async Task<ProfileViewModel?> HandleAsync(GetProfileQuery query, CancellationToken ct)
    {
        return await repository.GetByIdAsync<ProfileViewModel>([userContext.RequireUserId()], ct);
    }
}
