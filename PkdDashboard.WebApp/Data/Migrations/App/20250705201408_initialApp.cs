using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PkdDashboard.WebApp.Data.Migrations.App
{
    /// <inheritdoc />
    public partial class initialApp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "schPkd");

            migrationBuilder.CreateTable(
                name: "PkdEntries",
                schema: "schPkd",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Section = table.Column<char>(type: "character(1)", nullable: false),
                    Division = table.Column<int>(type: "integer", nullable: false),
                    Group = table.Column<int>(type: "integer", nullable: false),
                    Class = table.Column<int>(type: "integer", nullable: false),
                    PkdSuffix = table.Column<char>(type: "character(1)", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PkdEntries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CompanyCounts",
                schema: "schPkd",
                columns: table => new
                {
                    PkdEntryId = table.Column<int>(type: "integer", nullable: false),
                    Day = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Count = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanyCounts", x => new { x.PkdEntryId, x.Day });
                    table.ForeignKey(
                        name: "FK_CompanyCounts_PkdEntries_PkdEntryId",
                        column: x => x.PkdEntryId,
                        principalSchema: "schPkd",
                        principalTable: "PkdEntries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CompanyCounts",
                schema: "schPkd");

            migrationBuilder.DropTable(
                name: "PkdEntries",
                schema: "schPkd");
        }
    }
}
