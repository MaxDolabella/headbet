using HeadBet.Core.Domain.Enums;

namespace HeadBet.Core.Models;

public class AiModelListViewModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public AiProvider Provider { get; set; }
    public bool IsActive { get; set; }
}
