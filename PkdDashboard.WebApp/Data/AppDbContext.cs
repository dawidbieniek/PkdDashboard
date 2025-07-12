using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

using PkdDashboard.WebApp.Data.Abstract;
using PkdDashboard.WebApp.Data.Entities;

using System.Reflection.Emit;

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

        if (Database.IsNpgsql())
        {
            builder.Entity<PkdEntry>()
                .Property(e => e.PkdString)
                .HasComputedColumnSql(
                    "RIGHT('00' || CAST(\"Division\" AS VARCHAR(2)), 2) || '.' || CAST(\"Group\" AS VARCHAR(2)) || CAST(\"Class\" AS VARCHAR(2)) || '.' || CAST(\"PkdSuffix\" AS VARCHAR(1))",
                    stored: true);
        }
        else if (Database.IsSqlServer())
        {
            builder.Entity<PkdEntry>()
                .Property(e => e.PkdString)
                .HasComputedColumnSql(
                    "RIGHT('00' + CAST([Division] AS VARCHAR(2)), 2) + '.' + CAST([Group] AS VARCHAR(2)) + CAST([Class] AS VARCHAR(2)) + '.' + CAST([PkdSuffix] AS VARCHAR(1))",
                    stored: true);
        }

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
