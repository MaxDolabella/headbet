namespace HeadBet.Core.Domain.Interfaces;

/// <summary>Um e-mail aguardando envio em background.</summary>
public sealed record EmailJob(string To, string Subject, string HtmlBody);

/// <summary>
/// Fila de e-mails (producer). Enfileirar é instantâneo e não bloqueia a request;
/// o envio efetivo (com retry/timeout) acontece no EmailBackgroundService.
/// </summary>
public interface IEmailQueue
{
    /// <summary>
    /// Enfileira um e-mail para envio em background. Não bloqueia.
    /// Retorna <c>false</c> se a fila estiver cheia (o e-mail é descartado e logado).
    /// </summary>
    bool Enqueue(EmailJob job);
}
