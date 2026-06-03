namespace HeadBet.Core.Extensions;

public static class DateTimeExtensions
{
    private static readonly TimeZoneInfo _brt = TimeZoneInfo.FindSystemTimeZoneById(
        OperatingSystem.IsWindows() ? "E. South America Standard Time" : "America/Sao_Paulo");

    public static DateTime ToBrt(this DateTime utc)
        => TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utc, DateTimeKind.Utc), _brt);

    public static DateTime ToUtcFromBrt(this DateTime brt)
        => TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(brt, DateTimeKind.Unspecified), _brt);
}
