using HeadBet.Core.Domain.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HeadBet.Core.Infrastructure.Email;

/// <summary>
/// Consumidor da <see cref="EmailQueue"/>. Drena o canal e envia os e-mails com
/// concorrência limitada, retry e timeout por tentativa. Falhas viram log — nunca
/// derrubam o serviço nem afetam a request que enfileirou.
/// </summary>
public sealed class EmailBackgroundService(
    EmailQueue queue,
    IEmailSender sender,
    EmailSettings settings,
    ILogger<EmailBackgroundService> logger) : BackgroundService
{
    private const int MAX_CONCURRENCY = 4;
    private const int MAX_ATTEMPTS = 3;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var gate = new SemaphoreSlim(MAX_CONCURRENCY);

        try
        {
            await foreach (var job in queue.Reader.ReadAllAsync(stoppingToken))
            {
                await gate.WaitAsync(stoppingToken);
                // Dispara o envio sem segurar o loop; o gate limita a concorrência.
                _ = ProcessAsync(job, gate, stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            // Shutdown normal do host.
        }
    }

    private async Task ProcessAsync(EmailJob job, SemaphoreSlim gate, CancellationToken stoppingToken)
    {
        try
        {
            for (var attempt = 1; attempt <= MAX_ATTEMPTS; attempt++)
            {
                try
                {
                    using var attemptCts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
                    attemptCts.CancelAfter(TimeSpan.FromSeconds(settings.TimeoutSeconds));

                    logger.LogInformation("Enviando e-mail para {To}.", job.To);
                    await sender.SendAsync(job.To, job.Subject, job.HtmlBody, attemptCts.Token);
                    logger.LogInformation("E-mail enviado para {To}.", job.To);

                    return;
                }
                catch (Exception ex) when (attempt < MAX_ATTEMPTS && !stoppingToken.IsCancellationRequested)
                {
                    logger.LogWarning(ex,
                        "Falha ao enviar e-mail para {To} (tentativa {Attempt}/{Max}). Nova tentativa em instantes.",
                        job.To, attempt, MAX_ATTEMPTS);

                    await Task.Delay(TimeSpan.FromSeconds(2 * attempt), stoppingToken);
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Shutdown durante envio/espera — descarta silenciosamente.
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Falha definitiva ao enviar e-mail para {To} após {Max} tentativas.",
                job.To, MAX_ATTEMPTS);
        }
        finally
        {
            gate.Release();
        }
    }
}
