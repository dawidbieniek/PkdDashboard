using Microsoft.EntityFrameworkCore;

using PkdDashboard.WebApp.Data;
using PkdDashboard.WebApp.Data.Entities;
using PkdDashboard.WebApp.Services.Data;

namespace PkdDashboard.WebApp.Services;

internal class CompanyCountService(IDbContextFactory<AppDbContext> contextFactory)
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory = contextFactory;

    public async Task<List<DateTime>> GetAllDatesWithEntryAsync()
    {
        using var dbContext = _contextFactory.CreateDbContext();
        var distinctDays = await dbContext.CompanyCounts
            .Select(cc => cc.Day)
            .Distinct()
            .ToListAsync();

        // Converting to DateTime in memory, because SQL won't do this
        return [.. distinctDays.Select(day => day.ToDateTime(TimeOnly.MinValue))];
    }

    public async Task<PagedResult<CompanyCount>> GetListQueryAsync(DateOnly day, PagerSearchQuery pagerSearchQuery)
    {
        using var dbContext = _contextFactory.CreateDbContext();

        var query = dbContext.CompanyCounts
            .Where(cc => cc.Day == day)
            .Include(cc => cc.PkdEntry)
            .AsNoTracking()
            .MatchSearchQuery(ccx => ccx.PkdEntry.PkdString, pagerSearchQuery);

        var count = await query.CountAsync();
        var items = await query
            .OrderBy(cc => cc.PkdEntry.Division)
            .ThenBy(cc => cc.PkdEntry.Group)
            .ThenBy(cc => cc.PkdEntry.Class)
            .SkipTake(pagerSearchQuery)
            .ToListAsync();

        return new(items, count);
    }
}
