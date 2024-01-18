using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    public partial class addOpaqueTokenToUsersTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OpaqueToken",
                table: "Users",
                type: "nvarchar(36)",
                maxLength: 36,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_OpaqueToken",
                table: "Users",
                column: "OpaqueToken",
                unique: true,
                filter: "[OpaqueToken] IS NOT NULL");

            migrationBuilder.AddColumn<DateTime>(
                name: "OpaqueTokenExpiration",
                table: "Users",
                type: "datetime",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OpaqueToken",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "OpaqueTokenExpiration",
                table: "Users");
        }
    }
}
