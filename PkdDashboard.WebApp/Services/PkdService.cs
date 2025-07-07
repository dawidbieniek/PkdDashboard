using Microsoft.EntityFrameworkCore;

using PkdDashboard.WebApp.Data;
using PkdDashboard.WebApp.Data.Entities;

namespace PkdDashboard.WebApp.Services;

internal class PkdService(IDbContextFactory<AppDbContext> contextFactory)
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory = contextFactory;

    public IQueryable<PkdEntry> GetListQuery()
        => _contextFactory.CreateDbContext()
            .PkdEntries.AsNoTracking(); 
}