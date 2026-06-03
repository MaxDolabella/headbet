using Headsoft.Core.Entities;

namespace HeadBet.Core.Domain.Entities;

public class ChatThread : Entity<Guid>
{
    public Guid UserId { get; set; }
    public Guid AiModelId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string ThreadData { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public User User { get; set; } = null!;
    public AiModel AiModel { get; set; } = null!;
}
