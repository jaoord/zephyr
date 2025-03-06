using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Zephyr.Migrations
{
    /// <inheritdoc />
    public partial class Metar : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Metars",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Station = table.Column<string>(type: "TEXT", nullable: true),
                    MetarText = table.Column<string>(type: "TEXT", nullable: true),
                    TemperatureCelsius = table.Column<decimal>(type: "TEXT", nullable: true),
                    ObservationDateTimeZulu = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Metars", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Metars");
        }
    }
}
