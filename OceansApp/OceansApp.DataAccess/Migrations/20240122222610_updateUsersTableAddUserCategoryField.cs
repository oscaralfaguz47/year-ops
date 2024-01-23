using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    public partial class updateUsersTableAddUserCategoryField : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UserCategoryId",
                table: "Users",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_UserCategoryId",
                table: "Users",
                column: "UserCategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_UserCategories_UserCategoryId",
                table: "Users",
                column: "UserCategoryId",
                principalTable: "UserCategories",
                principalColumn: "UserCategoryId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_UserCategories_UserCategoryId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_UserCategoryId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "UserCategoryId",
                table: "Users");
        }
    }
}
