using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dipchik.ServiceDefaults.Migrations
{
    /// <inheritdoc />
    public partial class LanguaguagesChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CitiesJson",
                table: "Languages");

            migrationBuilder.RenameColumn(
                name: "CountriesJson",
                table: "Languages",
                newName: "CitiesAndCountriesJson");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CitiesAndCountriesJson",
                table: "Languages",
                newName: "CountriesJson");

            migrationBuilder.AddColumn<string>(
                name: "CitiesJson",
                table: "Languages",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
