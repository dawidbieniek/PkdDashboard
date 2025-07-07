using PkdDashboard.DataPollingService.Data;
using PkdDashboard.DataPollingService.Data.Entities;

namespace PkdDashboard.DataPollingService.Jobs.QueryCompanyCounts;

internal class DatabaseService(PkdDbContext dbContext)
{
    private readonly PkdDbContext _dbContext = dbContext;

    public bool IsTodayEntryPresent(DateOnly today)
        => _dbContext.CompanyCounts
            .Any(x => x.Day == today);

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0305:Simplify collection initialization", Justification = "<Pending>")]
    public List<PkdEntry> GetPkdsForQuery()
        => _dbContext.PkdEntries
            .Where(x => x.Id <= 5)  // HACK: For testing
            .ToList();

    public Task SaveCompanyCountAsync(DateOnly day, PkdEntry pkd, int count, CancellationToken cancellationToken)
    {
        var existingRecord = _dbContext.CompanyCounts
            .FirstOrDefault(x => x.Day == day && x.PkdEntryId == pkd.Id);

        if (existingRecord is not null)
        {
            existingRecord.Count = count;// TODO: Check if update is needed
            return _dbContext.SaveChangesAsync(cancellationToken);
        }

        var newRecord = new CompanyCount
        {
            Day = day,
            PkdEntryId = pkd.Id,
            Count = count
        };
        _dbContext.CompanyCounts.Add(newRecord);
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}