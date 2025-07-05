using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

using PkdDashboard.WebApp.Data.Abstract;
using PkdDashboard.WebApp.Data.Entities.Auth;

namespace PkdDashboard.WebApp.Data;

internal class AuthDbContext(DbContextOptions<AuthDbContext> options) : IdentityDbContext<ApplicationUser>(options), IDbContextWithMigrationTable
{
    private const string SchemaName = "schAuth";
    public static string MigrationsTable => "__AuthMigrationsHistory";

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasDefaultSchema(SchemaName);
        base.OnModelCreating(builder);
    }
}
