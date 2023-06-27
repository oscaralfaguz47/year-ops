using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    public partial class updateProviderCategoryTable2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PROVIDER_PROVIDER_CATEGORY_IdProviderCategory",
                table: "PROVIDER");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PROVIDER_CATEGORY",
                table: "PROVIDER_CATEGORY");

            migrationBuilder.DropIndex(
                name: "IX_PROVIDER_IdProviderCategory",
                table: "PROVIDER");

            migrationBuilder.DropColumn(
                name: "IdProviderCategory",
                table: "PROVIDER");

            migrationBuilder.RenameColumn(
                name: "IdProviderCategory",
                table: "PROVIDER_CATEGORY",
                newName: "ProviderCategoryCode");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "PROVIDER_CATEGORY",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "PROVIDER",
                type: "int",
                maxLength: 8,
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_PROVIDER_CATEGORY",
                table: "PROVIDER_CATEGORY",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_PROVIDER_Id",
                table: "PROVIDER",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PROVIDER_PROVIDER_CATEGORY_Id",
                table: "PROVIDER",
                column: "Id",
                principalTable: "PROVIDER_CATEGORY",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PROVIDER_PROVIDER_CATEGORY_Id",
                table: "PROVIDER");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PROVIDER_CATEGORY",
                table: "PROVIDER_CATEGORY");

            migrationBuilder.DropIndex(
                name: "IX_PROVIDER_Id",
                table: "PROVIDER");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "PROVIDER_CATEGORY");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "PROVIDER");

            migrationBuilder.RenameColumn(
                name: "ProviderCategoryCode",
                table: "PROVIDER_CATEGORY",
                newName: "IdProviderCategory");

            migrationBuilder.AddColumn<string>(
                name: "IdProviderCategory",
                table: "PROVIDER",
                type: "nvarchar(8)",
                maxLength: 8,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PROVIDER_CATEGORY",
                table: "PROVIDER_CATEGORY",
                column: "IdProviderCategory");

            migrationBuilder.CreateIndex(
                name: "IX_PROVIDER_IdProviderCategory",
                table: "PROVIDER",
                column: "IdProviderCategory");

            migrationBuilder.AddForeignKey(
                name: "FK_PROVIDER_PROVIDER_CATEGORY_IdProviderCategory",
                table: "PROVIDER",
                column: "IdProviderCategory",
                principalTable: "PROVIDER_CATEGORY",
                principalColumn: "IdProviderCategory",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
