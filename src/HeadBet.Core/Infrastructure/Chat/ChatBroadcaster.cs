using System.Collections.Concurrent;
using HeadBet.Core.Domain.Interfaces;

namespace HeadBet.Core.Infrastructure.Chat;

/// <summary>
/// Implementação em memória do <see cref="IChatBroadcaster"/>. Mantém os handlers
/// agrupados por chave de contexto; cada inscrição recebe um token para remoção O(1).
/// Thread-safe via <see cref="ConcurrentDictionary{TKey,TValue}"/> aninhado.
/// </summary>
public sealed class ChatBroadcaster : IChatBroadcaster
{
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<Guid, Func<ChatEvent, Task>>> _contexts = new();

    public IDisposable Subscribe(string contextKey, Func<ChatEvent, Task> handler)
    {
        var handlers = _contexts.GetOrAdd(contextKey, _ => new ConcurrentDictionary<Guid, Func<ChatEvent, Task>>());
        var token = Guid.NewGuid();
        handlers[token] = handler;
        return new Subscription(this, contextKey, token);
    }

    public async Task PublishAsync(ChatEvent evt)
    {
        if (!_contexts.TryGetValue(evt.ContextKey, out var handlers))
            return;

        // Snapshot dos valores: um handler que falhe não impede os demais.
        foreach (var handler in handlers.Values)
        {
            try
            {
                await handler(evt);
            }
            catch
            {
                // Circuito do inscrito pode ter morrido; ignora e segue.
            }
        }
    }

    private void Unsubscribe(string contextKey, Guid token)
    {
        if (_contexts.TryGetValue(contextKey, out var handlers))
        {
            handlers.TryRemove(token, out _);
            if (handlers.IsEmpty)
                _contexts.TryRemove(contextKey, out _);
        }
    }

    private sealed class Subscription(ChatBroadcaster owner, string contextKey, Guid token) : IDisposable
    {
        private bool _disposed;

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            owner.Unsubscribe(contextKey, token);
        }
    }
}
