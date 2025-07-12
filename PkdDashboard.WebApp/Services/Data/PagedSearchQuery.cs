using MudBlazor;

namespace PkdDashboard.WebApp.Services.Data;

public record PagerSearchQuery(PagerQuery PagerQuery, string? SearchQuery = null)
{
    public PagerSearchQuery(int page, int pageSize, string? searchQuery = null) : this(new PagerQuery(page, pageSize), searchQuery)
    { }

    public PagerSearchQuery(TableState tableState, string? searchQuery = null) : this(new PagerQuery(tableState), searchQuery)
    { }

    public bool ShouldSearch => !string.IsNullOrEmpty(SearchQuery?.Trim());
    public string NormalizedSearchQuery => SearchQuery?.ToLower().Trim() ?? string.Empty;
}