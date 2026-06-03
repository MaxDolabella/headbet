using Headsoft.Core.Entities;
using HeadBet.Core.Domain.Enums;

namespace HeadBet.Core.Domain.Entities;

public class AiModel : Entity<Guid>
{
    public AiProvider Provider { get; set; }
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}
