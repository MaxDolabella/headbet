using System.Text.Encodings.Web;
using Headsoft.Core;
using HeadBet.Core.Domain.Interfaces;
using HeadBet.Core.Models;

namespace HeadBet.Core.Extensions;

public static class NotificationExtensions
{
    /// <summary>
    /// Converte uma <see cref="Notification"/> num <see cref="NotificationView"/>
    /// pronto pra renderização, traduzindo o <c>Message</c> em título.
    /// </summary>
    public static NotificationView ToView(this Notification n, INotificationTranslator translator)
        => new()
        {
            Type = n.ResultType,
            Title = translator.Translate(n.Message),
            Details = n.Details,
        };

    /// <summary>
    /// Pega a primeira notificação de um <see cref="IOperationResult"/> como view.
    /// Retorna <c>null</c> se não houver notificação.
    /// </summary>
    public static NotificationView? FirstAsView(this IOperationResult result, INotificationTranslator translator)
    {
        var first = result.Notifications?.FirstOrDefault();
        return first?.ToView(translator);
    }

    /// <summary>
    /// Renderiza uma <see cref="Notification"/> como HTML pronto para alert/snackbar:
    /// <c>Message</c> é tratado como chave de tradução (ex.: <c>warning.common.item_not_found</c>),
    /// traduzido pelo <paramref name="translator"/> e exibido em negrito como título.
    /// <c>Details</c> vira o conteúdo. Ambos são HTML-encoded para evitar injeção.
    /// </summary>
    public static string ToAlertHtml(this Notification n, INotificationTranslator translator)
    {
        var rawTitle = translator.Translate(n.Message);
        var title = HtmlEncoder.Default.Encode(rawTitle);
        var details = HtmlEncoder.Default.Encode(n.Details ?? string.Empty);

        if (string.IsNullOrEmpty(title) && string.IsNullOrEmpty(details))
            return string.Empty;

        if (string.IsNullOrEmpty(title))
            return details;

        if (string.IsNullOrEmpty(details))
            return $"<strong>{title}</strong>";

        return $"<strong>{title}</strong><br/>{details}";
    }

    /// <summary>
    /// Pega a primeira notificação de um <see cref="IOperationResult"/> e a formata
    /// como HTML (título traduzido + conteúdo). Retorna o fallback (encoded) se não
    /// houver notificação.
    /// </summary>
    public static string FirstAsAlertHtml(this IOperationResult result, INotificationTranslator translator, string fallback)
    {
        var first = result.Notifications?.FirstOrDefault();
        if (first is null)
            return HtmlEncoder.Default.Encode(fallback);

        var html = first.ToAlertHtml(translator);
        return string.IsNullOrEmpty(html) ? HtmlEncoder.Default.Encode(fallback) : html;
    }
}
