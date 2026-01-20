using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PhotoGallery.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAppMetrics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppMetrics",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Key = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppMetrics", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "AppMetrics",
                columns: new[] { "Id", "Key", "UpdatedAtUtc", "Value" },
                values: new object[,]
                {
                    { 1, "TOTAL_UPLOADS", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 0L },
                    { 2, "TOTAL_DOWNLOADS", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 0L },
                    { 3, "TOTAL_DELETES", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 0L }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppMetrics_Key",
                table: "AppMetrics",
                column: "Key",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppMetrics");
        }
    }
}
