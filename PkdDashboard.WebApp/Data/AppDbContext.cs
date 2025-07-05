using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

using PkdDashboard.WebApp.Data.Abstract;
using PkdDashboard.WebApp.Data.Entities;

namespace PkdDashboard.WebApp.Data;

internal class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options), IDbContextWithMigrationTable
{
    private const string SchemaName = "schPkd";

    public static string MigrationsTable => "__AppMigrationsHistory";
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

internal class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        DbContextOptionsBuilder<AppDbContext> builder = new();
        builder.UseNpgsql();

        return new AppDbContext(builder.Options);
    }
}
