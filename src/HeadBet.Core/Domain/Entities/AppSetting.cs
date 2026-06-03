using Headsoft.Core.Entities;

namespace HeadBet.Core.Domain.Entities;

public class AppSetting : Entity<int>
{
    public string Name { get; set; } = string.Empty;
    public string Data { get; set; } = string.Empty;
}
