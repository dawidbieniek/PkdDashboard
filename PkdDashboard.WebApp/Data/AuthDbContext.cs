using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

using PkdDashboard.WebApp.Data.Entities.Auth;

namespace PkdDashboard.WebApp.Data;

public class AuthDbContext(DbContextOptions<AuthDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{
    private const string SchemaName = "schAuth";
    internal const string MigrationsTable = "__AuthMigrationsHistory";

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasDefaultSchema(SchemaName);
        base.OnModelCreating(builder);
    }
}
