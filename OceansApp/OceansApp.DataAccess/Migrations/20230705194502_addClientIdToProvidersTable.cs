using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    public partial class addClientIdToProvidersTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ClientId",
                table: "PROVIDER",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PROVIDER_ClientId",
                table: "PROVIDER",
                column: "ClientId");

            migrationBuilder.AddForeignKey(
                name: "FK_PROVIDER_CLIENT_ClientId",
                table: "PROVIDER",
                column: "ClientId",
                principalTable: "CLIENT",
                principalColumn: "ClientId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PROVIDER_CLIENT_ClientId",
                table: "PROVIDER");

            migrationBuilder.DropIndex(
                name: "IX_PROVIDER_ClientId",
                table: "PROVIDER");

            migrationBuilder.DropColumn(
                name: "ClientId",
                table: "PROVIDER");
        }
    }
}
