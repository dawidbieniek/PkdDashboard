namespace PkdDashboard.WebApp.Data.Entities;

internal class PkdEntry : Entity
{
    public char Section { get; set; }
    public int Division { get; set; }
    public int Group { get; set; }
    public int Class { get; set; }
    public char PkdSuffix { get; set; }
    public required string Description { get; set; }

    public string DivisionString => $"{Division:D2}";
    public string GroupString => $"{DivisionString}.{Group}";
    public string ClassString => $"{GroupString}.{Class}";
    public string PkdString => $"{ClassString}.{PkdSuffix}";

    public string ToQueryString => $"{Division:D2}{Group}{Class}{PkdSuffix}";
}
