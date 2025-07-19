using MudBlazor;

using System.ComponentModel.DataAnnotations.Schema;

namespace PkdDashboard.WebApp.Data.Entities;

internal class PkdEntry : Entity
{
    public char Section { get; set; }
    public int Division { get; set; }
    public int Group { get; set; }
    public int Class { get; set; }
    public char PkdSuffix { get; set; }
    public required string Description { get; set; }

    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public string PkdString { get; private set; } = default!;

    public string DivisionString => $"{Division:D2}";
    public string GroupString => $"{DivisionString}.{Group}";
    public string ClassString => $"{GroupString}{Class}";

    public string ToQueryString()
        => $"{Division:D2}{Group}{Class}{PkdSuffix}";

    public override bool Equals(object? obj)
    {
        return ReferenceEquals(this, obj) 
            || (obj is PkdEntry other 
                && Section == other.Section
                && Division == other.Division
                && Group == other.Group
                && Class == other.Class
                && PkdSuffix == other.PkdSuffix);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Section, Division, Group, Class, PkdSuffix);
    }
}