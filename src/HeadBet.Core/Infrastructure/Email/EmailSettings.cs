namespace HeadBet.Core.Infrastructure.Email;

/// <summary>
/// Configuração de SMTP, carregada da seção "Email" do appsettings.
/// </summary>
public sealed class EmailSettings
{
    public const string SECTION_NAME = "Email";

    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 587;
    public bool EnableSsl { get; set; } = true;
    public string User { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string From { get; set; } = string.Empty;
    public string FromName { get; set; } = "HeadBet";

    /// <summary>Timeout por tentativa de envio, em segundos.</summary>
    public int TimeoutSeconds { get; set; } = 30;

    public bool IsConfigured => !string.IsNullOrWhiteSpace(Host) && !string.IsNullOrWhiteSpace(From);
}
