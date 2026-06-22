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

        // Variação de posição = ranking só com placares reais (jogos FINALIZADOS) comparado
        // ao ranking atual (que também soma os jogos EM ANDAMENTO). Enquanto não há jogo ao
        // vivo, os dois rankings são idênticos e a variação é zero. Quando há, o baseline
        // ignora apenas os scores dos jogos que ainda não terminaram.
        var statusByMatch = (await matchRepository.ToListAsync(
                m => m.PoolId == query.PoolId, @readonly: true, ct))
            .ToDictionary(m => m.Id, m => m.Status);

        var hasLiveMatch = statusByMatch.Values.Any(s => s == MatchStatus.InProgress);

        var currentUserId = me?.UserId;

        // Prêmios/arrecadação são irrelevantes para o resumo — passamos vazio/zero.
        var current = PoolRankingCalculator.Compute(memberTuples, scores, [], pool.PrizeMode, 0m, currentUserId);

        var previous = !hasLiveMatch
            ? current
            : PoolRankingCalculator.Compute(
                memberTuples,
                scores.Where(s => statusByMatch.GetValueOrDefault(s.MatchId) == MatchStatus.Finished).ToList(),
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
