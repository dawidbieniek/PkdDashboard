using Microsoft.EntityFrameworkCore;

using PkdDashboard.DataPollingService.Data.Entities;

namespace PkdDashboard.DataPollingService.Data;

internal class PkdDbContext(DbContextOptions<PkdDbContext> options) : DbContext(options)
{
    private const string SchemaName = "schPkd";

    public DbSet<CompanyCount> CompanyCounts => Set<CompanyCount>();
    public DbSet<PkdEntry> PkdEntries => Set<PkdEntry>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasDefaultSchema(SchemaName);
        builder.Entity<CompanyCount>()
            .HasKey(cc => new { cc.PkdEntryId, cc.Day });
        base.OnModelCreating(builder);
    }
}