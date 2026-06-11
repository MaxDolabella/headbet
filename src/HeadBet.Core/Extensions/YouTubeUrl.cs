namespace HeadBet.Core.Extensions;

/// <summary>
/// Extrai o videoId de URLs do YouTube nos formatos watch?v=, youtu.be/, live/, shorts/ e embed/.
/// </summary>
public static class YouTubeUrl
{
    private const int VIDEO_ID_LENGTH = 11;
    private static readonly string[] PATH_PREFIXES = ["/live/", "/shorts/", "/embed/"];

    public static bool TryGetVideoId(string? url, out string videoId)
    {
        videoId = string.Empty;

        if (string.IsNullOrWhiteSpace(url))
            return false;

        if (!Uri.TryCreate(url.Trim(), UriKind.Absolute, out var uri)
            || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            return false;

        var host = uri.Host.ToLowerInvariant();
        string? candidate = null;

        if (host == "youtu.be")
        {
            candidate = FirstSegment(uri.AbsolutePath);
        }
        else if (host == "youtube.com" || host.EndsWith(".youtube.com"))
        {
            if (uri.AbsolutePath.Equals("/watch", StringComparison.OrdinalIgnoreCase))
            {
                candidate = uri.Query.TrimStart('?').Split('&')
                    .Where(p => p.StartsWith("v=", StringComparison.OrdinalIgnoreCase))
                    .Select(p => p[2..])
                    .FirstOrDefault();
            }
            else
            {
                foreach (var prefix in PATH_PREFIXES)
                {
                    if (uri.AbsolutePath.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                    {
                        candidate = FirstSegment(uri.AbsolutePath[prefix.Length..]);
                        break;
                    }
                }
            }
        }

        if (!IsValidVideoId(candidate))
            return false;

        videoId = candidate!;
        return true;
    }

    private static string FirstSegment(string path) =>
        path.Trim('/').Split('/')[0];

    private static bool IsValidVideoId(string? candidate) =>
        candidate?.Length == VIDEO_ID_LENGTH
        && candidate.All(c => char.IsAsciiLetterOrDigit(c) || c is '-' or '_');
}
