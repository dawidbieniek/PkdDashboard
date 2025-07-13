namespace PkdDashboard.Shared;

public static class MathExtensions
{
    public static DateTime Min(DateTime a, DateTime b) => a < b ? a : b;
    public static DateTime Max(DateTime a, DateTime b) => a > b ? a : b;
}