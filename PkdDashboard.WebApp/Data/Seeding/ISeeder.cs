using Microsoft.EntityFrameworkCore;

namespace PkdDashboard.WebApp.Data.Seeding;

internal interface ISeeder<T> where T : DbContext
{
    public Func<DbContext, bool, CancellationToken, Task> SeedAsync();
}
