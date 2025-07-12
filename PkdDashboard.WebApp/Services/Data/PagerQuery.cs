using MudBlazor;

namespace PkdDashboard.WebApp.Services.Data;

public record struct PagerQuery(int Page, int PageSize)
{
    public PagerQuery(TableState tableState) : this(tableState.Page, tableState.PageSize)
    { }
}