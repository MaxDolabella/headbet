namespace HeadBet.Core.Models;

public class UserApiKeyListViewModel
{
    public Guid Id { get; set; }
    public Guid AiModelId { get; set; }
    public string AiModelDisplayName { get; set; } = string.Empty;
    public string AiModelName { get; set; } = string.Empty;
    public string MaskedKey { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
    public DateTime CreatedAt { get; set; }
}
