using Microsoft.EntityFrameworkCore;

using PkdDashboard.WebApp.Data;
using PkdDashboard.WebApp.Data.Entities;

namespace PkdDashboard.WebApp.Jobs;

internal class FixCompanyCountsForDayJob(ILogger<FixCompanyCountsForDayJob> logger, AppDbContext dbContext) : IJob
{
    private readonly ILogger<FixCompanyCountsForDayJob> _logger = logger;
    private readonly AppDbContext _dbContext = dbContext;
    public static string JobId => nameof(FixCompanyCountsForDayJob);

    public async Task ExecuteAsync(DateOnly day, CancellationToken cancellationToken)
    {
        var prevDay = day.AddDays(-1);
        var prevCounts = await GetCountsForDay(prevDay, cancellationToken);

        if (prevCounts.Count == 0)
        {
            _logger.LogError("No counts found for previous day: {prevDay}. Cannot fix counts", prevDay);
            return;
        }

        var todayCounts = await GetCountsForDay(day, cancellationToken);

        if (prevCounts.Count == todayCounts.Count)
        {
            _logger.LogWarning("Counts for {day} match previous day. No action needed.", day);
            return;
        }

        var nextDay = day.AddDays(1);
        var nextCounts = await GetCountsForDay(nextDay, cancellationToken);

        if (nextCounts.Count == 0)
        {
            _logger.LogError("No counts found for next day: {nextDay}. Cannot fix counts", nextDay);
            return;
        }

        if (prevCounts.Count != nextCounts.Count)
        {
            _logger.LogError("Number of counts for previous and next day of {day} do not match. Cannot fix counts.", day);
            return;
        }

        if (todayCounts.Count > 0)
            _logger.LogInformation("Found {count} counts for {day}. Will fix missing ones.", todayCounts.Count, day);

        var missingCounts = new List<CompanyCount>(prevCounts.Count - todayCounts.Count);

        for (int i = todayCounts.Count; i < prevCounts.Count; i++)
        {
            var generatedCount = Interpolate(prevCounts[i].Count, nextCounts[i].Count);
            missingCounts.Add(new()
            {
                Count = generatedCount,
                Day = day,
                PkdEntryId = prevCounts[i].PkdEntryId,
            });
        }

        _logger.LogInformation("Generated {count} missing counts for {day}.", missingCounts.Count, day);

        await _dbContext.CompanyCounts.AddRangeAsync(missingCounts, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private Task<List<CompanyCount>> GetCountsForDay(DateOnly day, CancellationToken cancellationToken) => _dbContext.CompanyCounts
            .AsNoTracking()
            .Where(x => x.Day == day)
            .OrderBy(x => x.PkdEntryId)
            .ToListAsync(cancellationToken);

    private static int Interpolate(int a, int b, InterpolationMethod method = InterpolationMethod.Linear) => method switch
    {
        InterpolationMethod.Linear => (a + b) / 2,
        _ => throw new NotSupportedException($"Interpolation method {method} is not supported.")
    };

    private enum InterpolationMethod
    {
        Linear,
    }
}