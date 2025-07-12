using MudBlazor;

namespace PkdDashboard.WebApp.Services.Data;

public readonly record struct PagedResult<T>(IReadOnlyList<T> Items, int TotalCount);

public static class PagedResultExtensions
{
    public static TableData<T> ToTableData<T>(this PagedResult<T> pagedResult)
        => new()
        {
            TotalItems = pagedResult.TotalCount,
            Items = pagedResult.Items,
        };
}