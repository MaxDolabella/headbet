namespace HeadBet.Core.Models;

public class ChatMessageViewModel
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;

    /// <summary>Já chega em BRT (UTC-3). A View nunca chama <c>.ToLocalTime()</c>.</summary>
    public DateTime CreatedAt { get; set; }
}
