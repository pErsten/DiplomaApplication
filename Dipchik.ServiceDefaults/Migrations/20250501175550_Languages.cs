using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dipchik.ServiceDefaults.Migrations
{
    /// <inheritdoc />
    public partial class Languages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Roles",
                table: "Accounts",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Languages",
                columns: table => new
                {
                    Locale = table.Column<int>(type: "integer", nullable: false),
                    CountriesJson = table.Column<string>(type: "text", nullable: false),
                    CitiesJson = table.Column<string>(type: "text", nullable: false),
                    Json = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Languages", x => x.Locale);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Languages");

            migrationBuilder.DropColumn(
                name: "Roles",
                table: "Accounts");
        }
    }
}
