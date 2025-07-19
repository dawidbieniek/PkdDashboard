using MudBlazor;

namespace PkdDashboard.WebApp.Data.Dto;


internal record CompanyCountDiff(string PkdString, int CurrentCount, int PreviousCount)
{
    public int Difference => CurrentCount - PreviousCount;
}

internal static class CompanyCountDiffEnumerableExtensions
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0046:Convert to conditional expression", Justification = "<Pending>")]
    public static IEnumerable<CompanyCountDiff> OrderByTableState(this IEnumerable<CompanyCountDiff> query, TableState state)
    {
        var sortLabel = state.SortLabel;
        var sortDirection = state.SortDirection;

        if (sortDirection == SortDirection.None)
            return query.OrderBy(cc => cc.PkdString);

        return sortLabel switch
        {
            nameof(CompanyCountDiff.PkdString) => sortDirection == SortDirection.Ascending
                                ? query.OrderBy(cc => cc.PkdString)
                                : query.OrderByDescending(cc => cc.PkdString),
            nameof(CompanyCountDiff.PreviousCount) => sortDirection == SortDirection.Ascending
                                ? query.OrderBy(cc => cc.PreviousCount)
                                : query.OrderByDescending(cc => cc.PreviousCount),
            nameof(CompanyCountDiff.CurrentCount) => query = sortDirection == SortDirection.Ascending
                                ? query.OrderBy(cc => cc.CurrentCount)
                                : query.OrderByDescending(cc => cc.CurrentCount),
            nameof(CompanyCountDiff.Difference) => query = sortDirection == SortDirection.Ascending
                                ? query.OrderBy(cc => cc.Difference)
                                : query.OrderByDescending(cc => cc.Difference),
            _ => query.OrderBy(cc => cc.PkdString),
        };
    }
}