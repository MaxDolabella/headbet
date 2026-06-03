using Headsoft.Messaging.Abstractions.Queries;
using HeadBet.Core.Domain.Enums;
using HeadBet.Core.Domain.Interfaces;
using HeadBet.Core.Domain.Interfaces.Repositories;
using HeadBet.Core.Extensions;
using HeadBet.Core.Models;

namespace HeadBet.Core.Application.Queries;

// --- Query ---
public record GetMatchScoreFormQuery(Guid Id, Guid PoolId) : QueryBase<MatchScoreFormViewModel?>;

// --- Handler ---
public sealed class GetMatchScoreFormQueryHandler(
    IMatchRepository matchRepository,
    IPoolMemberRepository memberRepository,
    IUserContext userContext) : QueryHandlerBase<GetMatchScoreFormQuery, MatchScoreFormViewModel?>
{
    public override async Task<MatchScoreFormViewModel?> HandleAsync(GetMatchScoreFormQuery query, CancellationToken ct)
    {
        var isMember = await memberRepository.AnyAsync(
            m => m.PoolId == query.PoolId
                 && m.UserId == userContext.UserId
                 && m.Status == PoolMemberStatus.Active,
            ct);

        if (!isMember)
            return null;

        var vm = await matchRepository.GetAsync<MatchScoreFormViewModel>(
            m => m.Id == query.Id && m.PoolId == query.PoolId, ct);

        if (vm is null)
            return null;

        vm.MatchDate = vm.MatchDate.ToBrt();
        return vm;
    }
}
