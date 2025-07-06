namespace PkdDashboard.Shared.Migrations;

public interface IMigrator
{
    public Task EnsureDbContextCreatedAsync(CancellationToken cancellationToken);
    public Task<IEnumerable<string>> GetPendingMigrationsAsync(CancellationToken cancellationToken);
    public Task MigrateAsync(CancellationToken cancellationToken);
}
