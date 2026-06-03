using System.Threading.Channels;
using HeadBet.Core.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace HeadBet.Core.Infrastructure.Email;

/// <summary>
/// Fila de e-mails em memória sobre um <see cref="Channel{T}"/> limitado. Singleton:
/// o produtor (handlers) escreve via <see cref="Enqueue"/>; o consumidor
/// (<see cref="EmailBackgroundService"/>) lê via <see cref="Reader"/>.
/// </summary>
public sealed class EmailQueue : IEmailQueue
{
    private const int CAPACITY = 500;

    private readonly Channel<EmailJob> _channel;
    private readonly ILogger<EmailQueue> _logger;

    public EmailQueue(ILogger<EmailQueue> logger)
    {
        _logger = logger;
        _channel = Channel.CreateBounded<EmailJob>(new BoundedChannelOptions(CAPACITY)
        {
            // Wait + TryWrite: o produtor nunca espera; quando cheia, TryWrite devolve false.
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = false,
            SingleWriter = false,
        });
    }

    public ChannelReader<EmailJob> Reader => _channel.Reader;

    public bool Enqueue(EmailJob job)
    {
        if (_channel.Writer.TryWrite(job))
            return true;

        _logger.LogWarning(
            "Fila de e-mail cheia (capacidade {Capacity}). E-mail para {To} descartado.",
            CAPACITY, job.To);
        return false;
    }
}
