namespace HeadBet.Core.Domain.Interfaces;

/// <summary>
/// Traduz uma chave de mensagem (ex.: <c>warning.common.item_not_found</c>) para
/// um texto exibível em pt-BR. Quando a chave não está mapeada, retorna a própria
/// chave para não esconder o problema do desenvolvedor.
/// </summary>
public interface INotificationTranslator
{
    string Translate(string? key);
}
