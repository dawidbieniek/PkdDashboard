using Microsoft.EntityFrameworkCore;

using PkdDashboard.WebApp.Data;
using PkdDashboard.WebApp.Data.Entities;
using PkdDashboard.WebApp.Services.Data;

namespace PkdDashboard.WebApp.Services;

internal class PkdService(IDbContextFactory<AppDbContext> contextFactory)
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory = contextFactory;

    public async Task<List<PkdEntry>> GetAllAsync()
    {
        using var dbContext = _contextFactory.CreateDbContext();
        return await dbContext.PkdEntries
            .AsNoTracking()
            .OrderBy(pkd => pkd.Division)
            .ThenBy(pkd => pkd.Group)
            .ThenBy(pkd => pkd.Class)
            .ToListAsync();
    }

    public async Task<PagedResult<PkdEntry>> GetListQueryAsync(PagerSearchQuery pagerSearchQuery)
    {
        using var dbContext = _contextFactory.CreateDbContext();
            
        var query = dbContext.PkdEntries
            .AsNoTracking()
            .MatchSearchQuery(pkd => pkd.PkdString, pagerSearchQuery);

        var count = await query.CountAsync();
        var items = await query
            .OrderBy(pkd => pkd.Division)
            .ThenBy(pkd => pkd.Group)
            .ThenBy(pkd => pkd.Class)
            .SkipTake(pagerSearchQuery)
            .ToListAsync();

        return new(items, count);
    }
}