using Headsoft.Core;
using HeadBet.Core.Domain.Interfaces;
using HeadBet.Core.Extensions;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace HeadBet.Blazor.Infrastructure;

public static class SnackbarExtensions
{
    /// <summary>
    /// Adiciona uma <see cref="Notification"/> como snackbar, mapeando
    /// <see cref="Notification.ResultType"/> para <see cref="Severity"/> e
    /// usando <c>Message</c> (traduzido) como título em negrito e <c>Details</c>
    /// como conteúdo.
    /// </summary>
    public static void AddNotification(this ISnackbar snackbar, Notification n, INotificationTranslator translator)
    {
        var view = n.ToView(translator);

        var title = System.Net.WebUtility.HtmlEncode(view.Title);
        var details = System.Net.WebUtility.HtmlEncode(view.Details ?? string.Empty);

        string html;
        if (string.IsNullOrEmpty(title))
            html = details;
        else if (string.IsNullOrEmpty(details))
            html = $"<strong>{title}</strong>";
        else
            html = $"<strong>{title}</strong><br/>{details}";

        if (string.IsNullOrEmpty(html))
            return;

        snackbar.Add(new MarkupString(html), MapSeverity(view.Type));
    }

    private static Severity MapSeverity(ResultTypes type) => type switch
    {
        ResultTypes.Success => Severity.Success,
        ResultTypes.Info    => Severity.Info,
        ResultTypes.Warning => Severity.Warning,
        _                   => Severity.Error,
    };
}
