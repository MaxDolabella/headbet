using Headsoft.Core.Entities;
using HeadBet.Core.Domain.Enums;

namespace HeadBet.Core.Domain.Entities;

/// <summary>
/// Mensagem curta de chat dentro de um bolão. Uma única tabela serve às duas telas
/// (mural do bolão e comentários por jogo), diferenciadas pelo <see cref="Scope"/>.
/// </summary>
public class ChatMessage : Entity<Guid>
{
    public Guid PoolId { get; set; }
    public ChatScope Scope { get; set; }

    /// <summary>Preenchido apenas quando <see cref="Scope"/> é <see cref="ChatScope.Match"/>.</summary>
    public Guid? MatchId { get; set; }

    public Guid UserId { get; set; }
    public string Text { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public Guid? DeletedByUserId { get; set; }

    public Pool Pool { get; set; } = null!;
    public Match? Match { get; set; }
    public User User { get; set; } = null!;
}
