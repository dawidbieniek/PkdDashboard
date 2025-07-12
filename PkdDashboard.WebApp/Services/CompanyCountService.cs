using Microsoft.EntityFrameworkCore;

using PkdDashboard.WebApp.Data;
using PkdDashboard.WebApp.Data.Entities;

namespace PkdDashboard.WebApp.Services;

internal class CompanyCountService(IDbContextFactory<AppDbContext> contextFactory)
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory = contextFactory;

    public IQueryable<CompanyCount> GetListQuery(DateOnly day, string? searchQuery = null)
    {
        var query = _contextFactory.CreateDbContext().CompanyCounts
                .Where(cc => cc.Day == day)
                .Include(cc => cc.PkdEntry)
                .AsNoTracking();

        if (!string.IsNullOrEmpty(searchQuery))
        {
            var normalizedSearchQuery = searchQuery.ToLower().Trim();
            query = query.Where(cc => cc.PkdEntry.PkdString.ToLower().Contains(normalizedSearchQuery));
        }

        return query;
    }
}
