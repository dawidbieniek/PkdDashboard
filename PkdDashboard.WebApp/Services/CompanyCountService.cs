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
            .AsNoTracking()
            .Select(cc => cc.Day)
            .Distinct()
            .OrderBy(cc => cc)
            .ToListAsync();

        // Converting to DateTime in memory, because SQL won't do this
        return [.. distinctDays.Select(day => day.ToDateTime(TimeOnly.MinValue))];
    }

    public async Task<List<DateTime>> GetAllDatesWithMissingCounts()
    {
        using var dbContext = _contextFactory.CreateDbContext();

        var pkdExpectedCount = await dbContext.PkdEntries
            .AsNoTracking()
            .CountAsync();

        var daysWithFullCount = (await dbContext.CompanyCounts
            .AsNoTracking()
            .GroupBy(cc => cc.Day)
            .Where(g => g.Count() == pkdExpectedCount)
            .Select(g => g.Key)
            .Distinct()
            .OrderBy(cc => cc.Day)
            .ToListAsync())
            .Select(day => day.ToDateTime(TimeOnly.MinValue))
            .ToList();

        var lastDay = daysWithFullCount.Last();
        var firstDay = daysWithFullCount.First();

        var allDays = Enumerable.Range(0, (lastDay - firstDay).Days + 1)
            .Select(offset => firstDay.AddDays(offset))
            .ToList();

        return [.. allDays.Except(daysWithFullCount)];
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

    public async Task<List<CompanyCount>> GetCountsForPkdInDateRangeAsync(PkdEntry pkd, DateOnly start, DateOnly end)
    {
        using var dbContext = _contextFactory.CreateDbContext();

        return await dbContext.CompanyCounts
            .Where(cc => cc.Day >= start && cc.Day <= end)
            .Where(cc => cc.PkdEntryId == pkd.Id)
            .OrderBy(cc => cc.Day)
            .AsNoTracking()
            .ToListAsync();
    }
}