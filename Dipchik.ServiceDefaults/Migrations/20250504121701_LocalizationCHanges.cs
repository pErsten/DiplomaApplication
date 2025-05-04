using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dipchik.ServiceDefaults.Migrations
{
    /// <inheritdoc />
    public partial class LocalizationCHanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Json",
                table: "Languages",
                newName: "DisplayLocalizationsJson");

            migrationBuilder.AddColumn<int>(
                name: "Locale",
                table: "Accounts",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Locale",
                table: "Accounts");

            migrationBuilder.RenameColumn(
                name: "DisplayLocalizationsJson",
                table: "Languages",
                newName: "Json");
        }
    }
}
