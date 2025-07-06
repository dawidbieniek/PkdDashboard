using CsvHelper.Configuration;

using PkdDashboard.WebApp.Data.Entities;

namespace PkdDashboard.WebApp.Data.Seeding.CsvMaps;

internal class PkdEntryMap : ClassMap<PkdEntry>
{
    public PkdEntryMap()
    {
        Map(m => m.Section).Name("Section");
        Map(m => m.Division).Name("Division");

        Map(m => m.Group)
            .Name("Group")
            .Convert(row =>
            {
                var text = row.Row.GetField("Group");
                return string.IsNullOrEmpty(text)
                    ? -1
                    : int.Parse(text.Split('.')[1]);
            });

        Map(m => m.Class)
            .Name("Class")
            .Convert(row =>
            {
                var text = row.Row.GetField("Class");
                return string.IsNullOrEmpty(text)
                    ? -1
                    : int.Parse(text.Split('.')[1]);
            });

        Map(m => m.PkdSuffix)
            .Name("PkdSuffix")
            .Convert(row =>
            {
                var text = row.Row.GetField("PkdSuffix") ?? string.Empty;
                return text.Trim().Last();
            });

        Map(m => m.Description).Name("Description");
    }
}