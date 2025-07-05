using PkdDashboard.Shared.Migrations;

namespace PkdDashboard.WebApp.Data;

internal class AppMigrator(AppDbContext dbContext) : MigratorBase<AppDbContext>(dbContext)
{ }
