namespace HeadBet.Core.Domain.Enums;

/// <summary>
/// Discriminador da <see cref="Entities.ChatMessage"/>: indica de qual tela a mensagem é.
/// </summary>
public enum ChatScope
{
    /// <summary>Mural geral do bolão (tela de detalhes).</summary>
    PoolGeneral = 1,

    /// <summary>Comentários de um jogo específico (tela de palpite).</summary>
    Match = 2,
}
