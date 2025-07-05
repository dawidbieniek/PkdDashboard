using PkdDashboard.Shared.Migrations;

namespace PkdDashboard.WebApp.Data;

internal class AuthMigrator(AuthDbContext dbContext) : MigratorBase<AuthDbContext>(dbContext)
{ }