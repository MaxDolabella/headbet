using Headsoft.Core;
using Headsoft.Core.Interfaces.Data;
using Headsoft.Messaging.Abstractions;
using Headsoft.Messaging.Abstractions.Commands;
using HeadBet.Core.Domain.Enums;
using HeadBet.Core.Domain.Interfaces;
using HeadBet.Core.Domain.Interfaces.Repositories;

namespace HeadBet.Core.Application.Commands;

// --- Command ---
public class DeletePoolCommand : CommandBase<OperationResult>
{
    public Guid Id { get; set; }
}

// --- Handler ---
public sealed class DeletePoolCommandHandler(
    IPoolRepository poolRepository,
    IPoolMemberRepository memberRepository,
    IMatchRepository matchRepository,
    IBetRepository betRepository,
    IMatchUserScoreRepository scoreRepository,
    ITeamRepository teamRepository,
    IPoolPrizeRepository prizeRepository,
    IPoolScoringRuleRepository scoringRuleRepository,
    IUnitOfWork uow,
    IUserContext userContext) : ICommandHandler<DeletePoolCommand, OperationResult>
{
    public async Task<OperationResult> HandleAsync(DeletePoolCommand command, CancellationToken ct)
    {
        var poolId = command.Id;

        var isAdmin = await memberRepository.AnyAsync(
            m => m.PoolId == poolId
                 && m.UserId == userContext.UserId
                 && m.Role == PoolMemberRole.Admin
                 && m.Status == PoolMemberStatus.Active,
            ct);

        if (!isAdmin)
            return Result.Warning(GenericMessages.INVALID_OPERATION, "Apenas o administrador pode apagar o bolão.");

        var hasBets = await betRepository.AnyAsync(b => b.Match.PoolId == poolId, ct);
        if (hasBets)
            return Result.Warning(GenericMessages.INVALID_OPERATION, "Não é possível apagar: existem palpites de usuários neste bolão.");

        var matches = await matchRepository.ToListAsync(m => m.PoolId == poolId, @readonly: true, ct);
        foreach (var match in matches)
        {
            var scores = await scoreRepository.ToListAsync(s => s.MatchId == match.Id, @readonly: true, ct);
            foreach (var score in scores)
                await scoreRepository.DeleteAsync([score.Id], ct);

            await matchRepository.DeleteAsync([match.Id], ct);
        }

        var teams = await teamRepository.ToListAsync(t => t.PoolId == poolId, @readonly: true, ct);
        foreach (var team in teams)
            await teamRepository.DeleteAsync([team.Id], ct);

        var members = await memberRepository.ToListAsync(m => m.PoolId == poolId, @readonly: true, ct);
        foreach (var member in members)
            await memberRepository.DeleteAsync([member.PoolId, member.UserId], ct);

        var prizes = await prizeRepository.ToListAsync(p => p.PoolId == poolId, @readonly: true, ct);
        foreach (var prize in prizes)
            await prizeRepository.DeleteAsync([prize.PoolId, prize.Position], ct);

        var rules = await scoringRuleRepository.ToListAsync(r => r.PoolId == poolId, @readonly: true, ct);
        foreach (var rule in rules)
            await scoringRuleRepository.DeleteAsync([rule.PoolId, rule.ScoreType], ct);

        await poolRepository.DeleteAsync([poolId], ct);
        return await uow.SaveChangesAsync(ct);
    }
}
