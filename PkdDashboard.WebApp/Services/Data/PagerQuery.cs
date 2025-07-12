using MudBlazor;

namespace PkdDashboard.WebApp.Services.Data;

public record struct PagerQuery(int Page, int PageSize)
{
    public static PagerQuery All => new(0, int.MaxValue);
    public PagerQuery(TableState tableState) : this(tableState.Page, tableState.PageSize)
    { }
}