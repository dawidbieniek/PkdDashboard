namespace PkdDashboard.Shared.DateUtil;

public static class DateUtil
{
    public static DateOnly Today
        => DateOnly.FromDateTime(DateTime.UtcNow.AddHours(2));
}