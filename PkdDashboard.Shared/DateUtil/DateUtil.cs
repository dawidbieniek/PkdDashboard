namespace PkdDashboard.Shared.DateUtil;

public static class DateUtil
{
    public static DateOnly TodayDate
        => DateOnly.FromDateTime(DateTime.UtcNow.AddHours(2));

    public static DateTime TodayDateTime
        => DateTime.UtcNow.AddHours(2).Date;
}