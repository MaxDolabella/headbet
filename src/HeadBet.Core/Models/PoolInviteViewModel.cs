using HeadBet.Core.Domain.Enums;

namespace HeadBet.Core.Models;

public class PoolInviteViewModel
{
    public Guid Id { get; set; }
    public string InviteCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsPaid { get; set; }
    public decimal? EntryFee { get; set; }
    public bool IsActive { get; set; }
    public bool IsPublic { get; set; }
    public int MemberCount { get; set; }

    /// <summary>
    /// Status atual do vínculo do usuário com o bolão. <c>null</c> = ainda não é membro.
    /// </summary>
    public PoolMemberStatus? MyStatus { get; set; }
}
