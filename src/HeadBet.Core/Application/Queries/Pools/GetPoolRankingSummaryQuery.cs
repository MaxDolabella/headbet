using Headsoft.Messaging.Abstractions.Queries;
using HeadBet.Core.Domain.Enums;
using HeadBet.Core.Domain.Interfaces;
using HeadBet.Core.Domain.Interfaces.Repositories;
using HeadBet.Core.Infrastructure.Scoring;
using HeadBet.Core.Models;

namespace HeadBet.Core.Application.Queries;

// --- Query ---
public record GetPoolRankingSummaryQuery(Guid PoolId) : QueryBase<List<RankingSummaryItemViewModel>>;

// --- Handler ---
// Resumo do ranking para a tela de detalhes do jogo. Calcula o ranking atual e,
// quando há jogos em andamento, também o ranking de antes deles (excluindo os
// scores das partidas InProgress) para derivar a variação de posição (chevron).
public sealed class GetPoolRankingSummaryQueryHandler(
    IPoolRepository poolRepository,
    IPoolMemberRepository memberRepository,
    IUserRepository userRepository,
    IMatchUserScoreRepository scoreRepository,
    IMatchRepository matchRepository,
    IUserContext userContext) : QueryHandlerBase<GetPoolRankingSummaryQuery, List<RankingSummaryItemViewModel>>
{
    public override async Task<List<RankingSummaryItemViewModel>> HandleAsync(GetPoolRankingSummaryQuery query, CancellationToken ct)
    {
        var pool = await poolRepository.GetAsync(p => p.Id == query.PoolId, @readonly: true, ct);
        if (pool is null)
            return [];

        var me = userContext.IsAuthenticated
            ? await memberRepository.GetAsync(
                m => m.PoolId == query.PoolId
                     && m.UserId == userContext.UserId
                     && m.Status == PoolMemberStatus.Active,
                @readonly: true, ct)
            : null;

        if (me is null && !pool.IsPublic)
            return [];

        var activeMembers = await memberRepository.ToListAsync(
            m => m.PoolId == query.PoolId && m.Status == PoolMemberStatus.Active,
            @readonly: true, ct);

        var memberUserIds = activeMembers.Select(m => m.UserId).ToList();
        var users = await userRepository.ToListAsync(u => memberUserIds.Contains(u.Id), @readonly: true, ct);
        var userNames = users.ToDictionary(u => u.Id, u => u.Name);

        var memberTuples = activeMembers
            .Select(m => (m.UserId, UserName: userNames.GetValueOrDefault(m.UserId, string.Empty)))
            .ToList();

        var scores = await scoreRepository.ToListAsync(s => s.Match.PoolId == query.PoolId, @readonly: true, ct);

        var inProgressIds = (await matchRepository.ToListAsync(
                m => m.PoolId == query.PoolId && m.Status == MatchStatus.InProgress, @readonly: true, ct))
            .Select(m => m.Id)
            .ToHashSet();

        var currentUserId = me?.UserId;

        // Prêmios/arrecadação são irrelevantes para o resumo — passamos vazio/zero.
        var current = PoolRankingCalculator.Compute(memberTuples, scores, [], pool.PrizeMode, 0m, currentUserId);

        var previous = inProgressIds.Count == 0
            ? current
            : PoolRankingCalculator.Compute(
                memberTuples,
                scores.Where(s => !inProgressIds.Contains(s.MatchId)).ToList(),
                [], pool.PrizeMode, 0m, currentUserId);

        var prevPositionByUser = previous.ToDictionary(i => i.UserId, i => i.Position);

        return current.Select(c => new RankingSummaryItemViewModel
        {
            Position = c.Position,
            UserName = c.UserName,
            TotalPoints = c.TotalPoints,
            IsCurrentUser = c.IsCurrentUser,
            PositionDelta = prevPositionByUser.TryGetValue(c.UserId, out var prev) ? prev - c.Position : 0,
        }).ToList();
    }
}
