namespace PkdDashboard.DataPollingService.Data.Entities;

internal class CompanyCount
{
    public int PkdEntryId { get; set; }
    public virtual PkdEntry PkdEntry { get; set; } = default!;
    public DateOnly Day { get; set; }
    public int Count { get; set; }
}
