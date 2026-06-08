using HeadBet.Core.Domain.Enums;
using HeadBet.Core.Models;

namespace HeadBet.Core.Domain.Interfaces;

/// <summary>
/// Evento de chat publicado para os componentes inscritos num contexto.
/// Mensagem nova traz <see cref="Message"/>; remoção traz <see cref="DeletedMessageId"/>.
/// </summary>
public sealed record ChatEvent(string ContextKey, ChatMessageViewModel? Message, Guid? DeletedMessageId)
{
    public bool IsDelete => DeletedMessageId.HasValue;
}

/// <summary>
/// Pub/sub em memória para chat em tempo real. Como o Blazor Server roda num único
/// processo com um circuito por usuário, basta um singleton distribuindo eventos —
/// sem necessidade de SignalR dedicado.
/// </summary>
public interface IChatBroadcaster
{
    /// <summary>Inscreve um handler num contexto. Faça <c>Dispose</c> no descarte do componente.</summary>
    IDisposable Subscribe(string contextKey, Func<ChatEvent, Task> handler);

    /// <summary>Entrega o evento a todos os inscritos no <see cref="ChatEvent.ContextKey"/>.</summary>
    Task PublishAsync(ChatEvent evt);
}

/// <summary>
/// Constrói a chave de contexto compartilhada entre quem publica (handler) e quem
/// se inscreve (componente). Mural do bolão e comentários de jogo são contextos distintos.
/// </summary>
public static class ChatContextKeys
{
    public static string For(ChatScope scope, Guid poolId, Guid? matchId) =>
        scope == ChatScope.Match
            ? $"match:{matchId}"
            : $"pool:{poolId}";
}
