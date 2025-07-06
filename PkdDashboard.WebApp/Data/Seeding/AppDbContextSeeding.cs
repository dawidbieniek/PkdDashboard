using CsvHelper;
using CsvHelper.Configuration;

using Microsoft.EntityFrameworkCore;

using PkdDashboard.WebApp.Data.Entities;
using PkdDashboard.WebApp.Data.Seeding.CsvMaps;

using System.Globalization;
using System.Reflection;

namespace PkdDashboard.WebApp.Data.Seeding;

internal class AppDbContextSeeding(ILogger<AppDbContextSeeding> logger) : ISeeder<AppDbContext>
{
    private readonly ILogger<AppDbContextSeeding> _logger = logger;

    private readonly HashSet<string> _resourcesToConsider = new()
    {
        "PkdDashboard.Migrator.Resources.Seeding.AppDbContext.PkdEntry.tsv"
    };


    public Func<DbContext, bool, CancellationToken, Task> SeedAsync() => async (context, _, cancellationToken) =>
    {
        if (context is not AppDbContext appDbContext)
            throw new InvalidOperationException($"Invalid context type. Expected {nameof(AppDbContext)}.");

        _logger.LogInformation("Starting seeding");

        var assembly = Assembly.GetEntryAssembly();
        if (assembly is null)
        {
            _logger.LogError("No entry assembly found. Cannot seed resources.");
            return;
        }

        var names = assembly.GetManifestResourceNames();

        foreach (var name in names)
        {
            if (!_resourcesToConsider.Contains(name))
                continue;

            _logger.LogInformation("Seeding resource: {ResourceName}", name);
            using var stream = assembly.GetManifestResourceStream(name)!;
            await SeedFromFileAsync(stream, appDbContext, cancellationToken);
        }

        _logger.LogInformation("Finished seeding");
    };

    private static async Task SeedFromFileAsync(Stream stream, AppDbContext context, CancellationToken cancellationToken)
    {
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            Delimiter = "\t",
            WhiteSpaceChars = [], // Disable whitespace characters to avoid issues with tab-delimited files
        };

        using var csv = new CsvReader(new StreamReader(stream), config);
        csv.Context.RegisterClassMap<PkdEntryMap>();

        var recordsInFile = csv.GetRecordsAsync<PkdEntry>(cancellationToken);

        var recordsInDatabase = await context.PkdEntries.ToHashSetAsync(cancellationToken);

        List<PkdEntry> recordsToAdd = [];
        await foreach (var record in recordsInFile)
        {
            if (!recordsInDatabase.Contains(record))
                recordsToAdd.Add(record);
        }

        await context.PkdEntries.AddRangeAsync(recordsToAdd, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }
}