namespace HeadBet.Core.Domain.Enums;

public enum JoinSource
{
    /// <summary>
    /// Usuário clicou em "Entrar" na listagem de bolões públicos.
    /// Respeita o flag <c>AutoAccept</c> do bolão.
    /// </summary>
    EnterButton = 1,

    /// <summary>
    /// Usuário acessou o link de convite direto. Sempre entra como Active.
    /// </summary>
    InviteLink = 2,
}
