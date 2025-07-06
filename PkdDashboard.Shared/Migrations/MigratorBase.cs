using Microsoft.EntityFrameworkCore;

namespace PkdDashboard.Shared.Migrations;

public abstract class MigratorBase<T>(T dbContext) : IMigrator where T : DbContext
{
    protected T DbContext { get; private init; } = dbContext;

    public async Task EnsureDbContextCreatedAsync(CancellationToken cancellationToken)
    {
        var strategy = DbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await DbContext.Database.EnsureCreatedAsync(cancellationToken);
        });
    }

    public Task<IEnumerable<string>> GetPendingMigrationsAsync(CancellationToken cancellationToken) => DbContext.Database.GetPendingMigrationsAsync(cancellationToken);

    public async Task MigrateAsync(CancellationToken cancellationToken)
    {
        var strategy = DbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await DbContext.Database.MigrateAsync(cancellationToken);
        });
    }
}