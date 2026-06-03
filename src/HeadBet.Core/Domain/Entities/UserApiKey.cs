using Headsoft.Core.Entities;

namespace HeadBet.Core.Domain.Entities;

public class UserApiKey : Entity<Guid>
{
    public Guid UserId { get; set; }
    public Guid AiModelId { get; set; }
    public string ApiKey { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
    public DateTime CreatedAt { get; set; }

    public User User { get; set; } = null!;
    public AiModel AiModel { get; set; } = null!;
}
