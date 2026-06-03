using Headsoft.Core;

namespace HeadBet.Core.Models;

/// <summary>
/// Modelo de exibição de uma <see cref="Notification"/>: já carrega o
/// <see cref="ResultTypes"/> para cor, o <see cref="Title"/> traduzido
/// e o <see cref="Details"/> como conteúdo.
/// </summary>
public sealed class NotificationView
{
    public ResultTypes Type { get; set; } = ResultTypes.Error;
    public string Title { get; set; } = string.Empty;
    public string? Details { get; set; }
}
