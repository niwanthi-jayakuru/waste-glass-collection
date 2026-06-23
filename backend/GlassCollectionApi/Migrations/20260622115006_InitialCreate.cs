using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GlassCollectionApi.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Suppliers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Address = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Latitude = table.Column<double>(type: "REAL", nullable: false),
                    Longitude = table.Column<double>(type: "REAL", nullable: false),
                    ExpectedClearKg = table.Column<decimal>(type: "TEXT", precision: 10, scale: 2, nullable: false),
                    ExpectedColouredKg = table.Column<decimal>(type: "TEXT", precision: 10, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Suppliers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Trips",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Date = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    StartLatitude = table.Column<double>(type: "REAL", nullable: false),
                    StartLongitude = table.Column<double>(type: "REAL", nullable: false),
                    TotalDistanceKm = table.Column<decimal>(type: "TEXT", precision: 10, scale: 2, nullable: false),
                    StartedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Trips", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TripStops",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TripId = table.Column<int>(type: "INTEGER", nullable: false),
                    SupplierId = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    SequenceOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    CollectedClearKg = table.Column<decimal>(type: "TEXT", precision: 10, scale: 2, nullable: true),
                    CollectedColouredKg = table.Column<decimal>(type: "TEXT", precision: 10, scale: 2, nullable: true),
                    Condition = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    CollectedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TripStops", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TripStops_Suppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Suppliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TripStops_Trips_TripId",
                        column: x => x.TripId,
                        principalTable: "Trips",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Trips_Date",
                table: "Trips",
                column: "Date");

            migrationBuilder.CreateIndex(
                name: "IX_TripStops_SupplierId",
                table: "TripStops",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_TripStops_TripId_SequenceOrder",
                table: "TripStops",
                columns: new[] { "TripId", "SequenceOrder" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TripStops_TripId_SupplierId",
                table: "TripStops",
                columns: new[] { "TripId", "SupplierId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TripStops");

            migrationBuilder.DropTable(
                name: "Suppliers");

            migrationBuilder.DropTable(
                name: "Trips");
        }
    }
}
