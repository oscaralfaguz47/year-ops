using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddConversionOrigin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OriginId",
                table: "TODOS",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OriginType",
                table: "TODOS",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OriginId",
                table: "ISSUES",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OriginType",
                table: "ISSUES",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TODOS_OriginType_OriginId",
                table: "TODOS",
                columns: new[] { "OriginType", "OriginId" });

            migrationBuilder.CreateIndex(
                name: "IX_ISSUES_OriginType_OriginId",
                table: "ISSUES",
                columns: new[] { "OriginType", "OriginId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TODOS_OriginType_OriginId",
                table: "TODOS");

            migrationBuilder.DropIndex(
                name: "IX_ISSUES_OriginType_OriginId",
                table: "ISSUES");

            migrationBuilder.DropColumn(
                name: "OriginId",
                table: "TODOS");

            migrationBuilder.DropColumn(
                name: "OriginType",
                table: "TODOS");

            migrationBuilder.DropColumn(
                name: "OriginId",
                table: "ISSUES");

            migrationBuilder.DropColumn(
                name: "OriginType",
                table: "ISSUES");
        }
    }
}
