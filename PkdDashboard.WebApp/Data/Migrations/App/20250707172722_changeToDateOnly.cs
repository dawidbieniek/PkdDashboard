using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PkdDashboard.WebApp.Data.Migrations.App
{
    /// <inheritdoc />
    public partial class changeToDateOnly : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateOnly>(
                name: "Day",
                schema: "schPkd",
                table: "CompanyCounts",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "Day",
                schema: "schPkd",
                table: "CompanyCounts",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateOnly),
                oldType: "date");
        }
    }
}
