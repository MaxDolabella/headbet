using Headsoft.Core.Sorting;
using Headsoft.Messaging.Abstractions.Queries;
using HeadBet.Core.Domain.Enums;
using HeadBet.Core.Domain.Interfaces;
using HeadBet.Core.Domain.Interfaces.Repositories;
using HeadBet.Core.Extensions;
using HeadBet.Core.Models;

namespace HeadBet.Core.Application.Queries;

// --- Query ---
public record ListChatMessagesQuery(Guid PoolId, ChatScope Scope, Guid? MatchId = null)
    : QueryBase<List<ChatMessageViewModel>>;

// --- Handler ---
public sealed class ListChatMessagesQueryHandler(
    IPoolMemberRepository memberRepository,
    IChatMessageRepository chatRepository,
    IUserContext userContext) : QueryHandlerBase<ListChatMessagesQuery, List<ChatMessageViewModel>>
{
    // Histórico exibido por contexto. Bolão de amigos não chega perto disso,
    // mas evita carregar conversas longas de uma vez.
    private const int MAX_MESSAGES = 50;

    public override async Task<List<ChatMessageViewModel>> HandleAsync(ListChatMessagesQuery query, CancellationToken ct)
    {
        var userId = userContext.UserId;

        var isMember = await memberRepository.AnyAsync(
            m => m.PoolId == query.PoolId
                 && m.UserId == userId
                 && m.Status == PoolMemberStatus.Active,
            ct);

        if (!isMember)
            return [];

        var matchId = query.Scope == ChatScope.Match ? query.MatchId : null;

        var messages = await chatRepository.ToListAsync<ChatMessageViewModel>(
            m => m.PoolId == query.PoolId
                 && m.Scope == query.Scope
                 && m.MatchId == matchId
                 && !m.IsDeleted,
            null,
            m => m.CreatedAt,
            SortDirection.Ascending,
            ct);

        // As mais recentes, em ordem cronológica.
        var trimmed = messages.Count > MAX_MESSAGES
            ? messages.GetRange(messages.Count - MAX_MESSAGES, MAX_MESSAGES)
            : messages;

        foreach (var m in trimmed)
            m.CreatedAt = m.CreatedAt.ToBrt();

        return trimmed;
    }
}
