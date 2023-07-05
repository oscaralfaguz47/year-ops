using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    public partial class addProviderIdToProvidersTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_PROVIDER",
                table: "PROVIDER");

            migrationBuilder.RenameColumn(
                name: "IdProvider",
                table: "PROVIDER",
                newName: "ProviderCode");

            migrationBuilder.AddColumn<int>(
                name: "ProviderId",
                table: "PROVIDER",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PROVIDER",
                table: "PROVIDER",
                column: "ProviderId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_PROVIDER",
                table: "PROVIDER");

            migrationBuilder.DropColumn(
                name: "ProviderId",
                table: "PROVIDER");

            migrationBuilder.RenameColumn(
                name: "ProviderCode",
                table: "PROVIDER",
                newName: "IdProvider");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PROVIDER",
                table: "PROVIDER",
                column: "IdProvider");
        }
    }
}
