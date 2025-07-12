using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PkdDashboard.WebApp.Data.Migrations.App
{
    /// <inheritdoc />
    public partial class addPkdStringAsComputedProperty : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PkdString",
                schema: "schPkd",
                table: "PkdEntries",
                type: "text",
                nullable: false,
                computedColumnSql: "RIGHT('00' || CAST(\"Division\" AS VARCHAR(2)), 2) || '.' || CAST(\"Group\" AS VARCHAR(2)) || CAST(\"Class\" AS VARCHAR(2)) || '.' || CAST(\"PkdSuffix\" AS VARCHAR(1))",
                stored: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PkdString",
                schema: "schPkd",
                table: "PkdEntries");
        }
    }
}
