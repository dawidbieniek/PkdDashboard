namespace PkdDashboard.WebApp.Data.Abstract;

public interface IDbContextWithMigrationTable
{
    static abstract string MigrationsTable { get; }
}
