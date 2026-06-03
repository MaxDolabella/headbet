using Headsoft.Messaging.Abstractions.Queries;
using HeadBet.Core.Domain.Enums;
using HeadBet.Core.Domain.Interfaces;
using HeadBet.Core.Domain.Interfaces.Repositories;
using HeadBet.Core.Extensions;
using HeadBet.Core.Models;

namespace HeadBet.Core.Application.Queries;

// --- Query ---
public record GetMatchDetailsQuery(Guid PoolId, Guid MatchId) : QueryBase<MatchDetailsViewModel?>;

// --- Handler ---
public sealed class GetMatchDetailsQueryHandler(
    IPoolRepository poolRepository,
    IPoolMemberRepository memberRepository,
    IMatchRepository matchRepository,
    ITeamRepository teamRepository,
    IUserRepository userRepository,
    IBetRepository betRepository,
    IMatchUserScoreRepository scoreRepository,
    IUserContext userContext) : QueryHandlerBase<GetMatchDetailsQuery, MatchDetailsViewModel?>
{
    private const int BET_CUTOFF_MINUTES = 2;

    public override async Task<MatchDetailsViewModel?> HandleAsync(GetMatchDetailsQuery query, CancellationToken ct)
    {
        var pool = await poolRepository.GetAsync(p => p.Id == query.PoolId, @readonly: true, ct);
        if (pool is null)
            return null;

        var me = await memberRepository.GetAsync(
            m => m.PoolId == query.PoolId && m.UserId == userContext.UserId && m.Status == PoolMemberStatus.Active,
            @readonly: true, ct);

        if (me is null && !pool.IsPublic)
            return null;

        var match = await matchRepository.GetAsync(
            m => m.Id == query.MatchId && m.PoolId == query.PoolId, @readonly: true, ct);
        if (match is null)
            return null;

        var teams = await teamRepository.ToListAsync(
            t => t.Id == match.HomeTeamId || t.Id == match.AwayTeamId, @readonly: true, ct);

        var homeTeam = teams.FirstOrDefault(t => t.Id == match.HomeTeamId);
        var awayTeam = teams.FirstOrDefault(t => t.Id == match.AwayTeamId);

        var members = await memberRepository.ToListAsync(
            m => m.PoolId == query.PoolId && m.Status == PoolMemberStatus.Active, @readonly: true, ct);

        var memberUserIds = members.Select(m => m.UserId).ToList();

        var users = await userRepository.ToListAsync(
            u => memberUserIds.Contains(u.Id), @readonly: true, ct);

        var userNames = users.ToDictionary(u => u.Id, u => u.Name);

        var bets = await betRepository.ToListAsync(
            b => b.MatchId == query.MatchId, @readonly: true, ct);

        var scores = await scoreRepository.ToListAsync(
            s => s.MatchId == query.MatchId, @readonly: true, ct);

        var betByUser = bets.ToDictionary(b => b.UserId);
        var scoreByUser = scores.ToDictionary(s => s.UserId);

        var cutoff = match.MatchDate.AddMinutes(-BET_CUTOFF_MINUTES);
        var isBetPeriodOpen = DateTime.UtcNow < cutoff;

        var betItems = members.Select(member =>
        {
            betByUser.TryGetValue(member.UserId, out var bet);
            scoreByUser.TryGetValue(member.UserId, out var score);
            var isMe = member.UserId == userContext.UserId;

            return new BetListItemViewModel
            {
                UserId = member.UserId,
                UserName = userNames.GetValueOrDefault(member.UserId, string.Empty),
                HomeScore = bet?.HomeScore,
                AwayScore = bet?.AwayScore,
                Points = score?.Points ?? 0,
                AppliedRule = score?.AppliedRule,
                IsMe = isMe,
                CanShowBet = isMe || !isBetPeriodOpen,
            };
        })
        .OrderByDescending(i => i.CanShowBet ? i.Points : -1)
        .ThenBy(i => i.UserName)
        .ToList();

        var myBet = me is not null ? betByUser.GetValueOrDefault(me.UserId) : null;

        return new MatchDetailsViewModel
        {
            PoolId = pool.Id,
            PoolName = pool.Name,
            PoolIsPublic = pool.IsPublic,
            IsMember = me is not null,
            IsAdmin = me?.Role == PoolMemberRole.Admin,

            MatchId = match.Id,
            HomeTeamName = homeTeam?.Name ?? string.Empty,
            HomeTeamAbbreviation = homeTeam?.Abbreviation ?? string.Empty,
            HomeTeamFlagUrl = homeTeam?.FlagUrl ?? string.Empty,
            AwayTeamName = awayTeam?.Name ?? string.Empty,
            AwayTeamAbbreviation = awayTeam?.Abbreviation ?? string.Empty,
            AwayTeamFlagUrl = awayTeam?.FlagUrl ?? string.Empty,
            MatchDate = match.MatchDate.ToBrt(),
            HomeScore = match.HomeScore,
            AwayScore = match.AwayScore,
            Status = match.Status,
            Group = match.Group,
            Round = match.Round,

            IsBetPeriodOpen = isBetPeriodOpen,
            CanEditMyBet = me is not null && isBetPeriodOpen,
            MyBetHome = myBet?.HomeScore,
            MyBetAway = myBet?.AwayScore,

            Bets = betItems,
        };
    }
}
