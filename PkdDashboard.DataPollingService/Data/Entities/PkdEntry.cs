namespace PkdDashboard.DataPollingService.Data.Entities;

internal class PkdEntry
{
    public int Id { get; set; }
    public int Division { get; set; }
    public int Group { get; set; }
    public int Class { get; set; }
    public char PkdSuffix { get; set; }

    public string ToQueryString => $"{Division:D2}{Group}{Class}{PkdSuffix}";
}