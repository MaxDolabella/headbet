using Headsoft.Messaging.Abstractions;
using Headsoft.Messaging.Abstractions.Commands;
using Headsoft.Messaging.Abstractions.Queries;

namespace HeadBet.Blazor.Infrastructure;

/// <summary>
/// Wrappers de <see cref="IBus.SendAsync"/> sem CancellationToken obrigatório.
/// Em Blazor Server o ciclo de vida do componente cobre o cancelamento real;
/// chamadas curtas usam <see cref="CancellationToken.None"/>.
/// </summary>
public static class BusExtensions
{
    public static Task<TResult> SendAsync<TResult>(this IBus bus, CommandBase<TResult> command)
        => bus.SendAsync(command, CancellationToken.None);

    public static Task<TResult> SendAsync<TResult>(this IBus bus, QueryBase<TResult> query)
        => bus.SendAsync(query, CancellationToken.None);
}
