namespace PkdDashboard.WebApp.Data.Entities;

internal class CompanyCount
{
    public int PkdEntryId { get; set; }
    public virtual PkdEntry PkdEntry { get; set; } = default!;
    public DateOnly Day { get; set; }
    public int Count { get; set; }

    public static int operator -(CompanyCount left, CompanyCount right)
    {
        if (left.PkdEntryId != right.PkdEntryId)
            throw new InvalidOperationException($"Cannot subtract {nameof(CompanyCount)}s with different {nameof(PkdEntryId)}s.");
        return left.Count - right.Count;
    }
}