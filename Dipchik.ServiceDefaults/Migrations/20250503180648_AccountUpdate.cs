using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dipchik.ServiceDefaults.Migrations
{
    /// <inheritdoc />
    public partial class AccountUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AvatarUrl",
                table: "Accounts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Accounts",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Username",
                table: "Accounts",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "UtcDeleted",
                table: "Accounts",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AvatarUrl",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "Username",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "UtcDeleted",
                table: "Accounts");
        }
    }
}
