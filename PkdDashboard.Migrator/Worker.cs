using OpenTelemetry.Trace;

using PkdDashboard.Shared.Migrations;

using System.Diagnostics;

namespace PkdDashboard.Migrator;

internal class Worker(IServiceProvider serviceProvider, IHostApplicationLifetime hostApplicationLifetime) : BackgroundService
{
    public const string ActivitySourceName = "Database migrator";
    private static readonly ActivitySource s_activitySource = new(ActivitySourceName);

    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private readonly IHostApplicationLifetime _hostApplicationLifetime = hostApplicationLifetime;

    private readonly Lazy<ILogger<Worker>> _logger = new(serviceProvider.GetRequiredService<ILogger<Worker>>);

    /// <summary>
    /// Runs all discovered migrators in the application.
    /// </summary>
    /// <remarks> Will not continue other migrations even if some previous fail </remarks>
    /// <exception cref="AggregateException"> </exception>
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        using Activity? activity = s_activitySource.StartActivity("Migrating database", ActivityKind.Client);
        using IServiceScope scope = _serviceProvider.CreateScope();

        IEnumerable<IMigrator> migratorsCollection;
        try
        {
            migratorsCollection = scope.ServiceProvider.GetServices<IMigrator>();

            _logger.Value.LogInformation("Found {MigratorCount} migrators", migratorsCollection.Count());
            foreach (IMigrator migrator in migratorsCollection)
                _logger.Value.LogDebug("Found migrator: {MigratorName}", migrator.GetType().Name);
        }
        catch (Exception ex)
        {
            activity?.RecordException(ex);
            throw;
        }

        var exceptions = new List<Exception>();

        foreach (IMigrator migrator in migratorsCollection)
        {
            string migratorName = migrator.GetType().Name;

            try
            {
                _logger.Value.LogInformation("Processing {MigratorName}", migratorName);

                IEnumerable<string> pendingMigrations = await migrator.GetPendingMigrationsAsync(cancellationToken);

                if (pendingMigrations.Any())
                {
                    await PerformMigrationsAsync(migrator, cancellationToken);

                    foreach (string migration in pendingMigrations)
                        _logger.Value.LogDebug("Migrations applied: {Migration}", migration);
                }
                else
                    _logger.Value.LogInformation("No migrations to apply for {MigratorName}", migratorName);

                await migrator.EnsureDbContextCreatedAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                activity?.RecordException(ex);
                _logger.Value.LogError(ex, "Migration failed for {MigratorName}", migratorName);
                exceptions.Add(ex);
            }
        }

        if (exceptions.Count > 0)
            throw new AggregateException("One or more migrations failed.", exceptions);

        _logger.Value.LogInformation("All migrations completed successfully.");
        _hostApplicationLifetime.StopApplication();
    }

    private async Task PerformMigrationsAsync(IMigrator migrator, CancellationToken cancellationToken)
    {
        _logger.Value.LogInformation("Performing migrations using {MigratorName}", migrator.GetType().Name);
        await migrator.MigrateAsync(cancellationToken);
    }
}