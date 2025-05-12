using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dipchik.ServiceDefaults.Migrations
{
    /// <inheritdoc />
    public partial class ChangeCancellationOfTourStorage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "TourInstances");

            migrationBuilder.AddColumn<bool>(
                name: "IsCancelled",
                table: "TourInstances",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsCancelled",
                table: "TourInstances");

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "TourInstances",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
