namespace HeadBet.Core.Domain.Interfaces;

/// <summary>
/// Abstração de envio de e-mail. Implementação em Infrastructure usa SMTP
/// (System.Net.Mail) configurado pela seção "Email" do appsettings.
/// </summary>
public interface IEmailSender
{
    Task SendAsync(string to, string subject, string htmlBody, CancellationToken ct = default);
}
