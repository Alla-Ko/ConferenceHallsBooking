using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConferenceHalls.Migrations
{
    /// <inheritdoc />
    public partial class AddConferenceHall : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ConferenceHalls",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Capacity = table.Column<int>(type: "INTEGER", nullable: false),
                    HourlyRate = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConferenceHalls", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ConferenceHallServices",
                columns: table => new
                {
                    ConferenceHallId = table.Column<int>(type: "INTEGER", nullable: false),
                    HallServiceId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConferenceHallServices", x => new { x.ConferenceHallId, x.HallServiceId });
                    table.ForeignKey(
                        name: "FK_ConferenceHallServices_ConferenceHalls_ConferenceHallId",
                        column: x => x.ConferenceHallId,
                        principalTable: "ConferenceHalls",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ConferenceHallServices_HallServices_HallServiceId",
                        column: x => x.HallServiceId,
                        principalTable: "HallServices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ConferenceHallServices_HallServiceId",
                table: "ConferenceHallServices",
                column: "HallServiceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConferenceHallServices");

            migrationBuilder.DropTable(
                name: "ConferenceHalls");
        }
    }
}
