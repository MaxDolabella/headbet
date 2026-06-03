using System.Net;
using System.Net.Mail;
using HeadBet.Core.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace HeadBet.Core.Infrastructure.Email;

/// <summary>
/// Envio de e-mail via SMTP usando <see cref="SmtpClient"/> (System.Net.Mail).
/// </summary>
public sealed class SmtpEmailSender(EmailSettings settings, ILogger<SmtpEmailSender> logger) : IEmailSender
{
    public async Task SendAsync(string to, string subject, string htmlBody, CancellationToken ct = default)
    {
        if (!settings.IsConfigured)
        {
            logger.LogWarning(
                "SMTP não configurado (seção Email do appsettings). E-mail para {To} com assunto '{Subject}' não foi enviado.",
                to, subject);
            return;
        }

        using var message = new MailMessage
        {
            From = new MailAddress(settings.From, settings.FromName),
            Subject = subject,
            Body = htmlBody,
            IsBodyHtml = true,
        };
        message.To.Add(to);

        using var client = new SmtpClient(settings.Host, settings.Port)
        {
            EnableSsl = settings.EnableSsl,
            Credentials = new NetworkCredential(settings.User, settings.Password),
            Timeout = (settings.TimeoutSeconds <= 0 ? 30 : settings.TimeoutSeconds) * 1000,
        };

        // O ct (com prazo via CancellationToken) é a defesa principal contra servidor travado;
        // Timeout acima é o cinto de segurança no caminho síncrono do SmtpClient.
        await client.SendMailAsync(message, ct);
    }
}
