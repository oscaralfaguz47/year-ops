using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    public partial class addSentByUserToNotificationsTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SentByUser",
                table: "NOTIFICATIONS",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_NOTIFICATIONS_SentByUser",
                table: "NOTIFICATIONS",
                column: "SentByUser");

            migrationBuilder.AddForeignKey(
                name: "FK_NOTIFICATIONS_Users_SentByUser",
                table: "NOTIFICATIONS",
                column: "SentByUser",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NOTIFICATIONS_Users_SentByUser",
                table: "NOTIFICATIONS");

            migrationBuilder.DropIndex(
                name: "IX_NOTIFICATIONS_SentByUser",
                table: "NOTIFICATIONS");

            migrationBuilder.DropColumn(
                name: "SentByUser",
                table: "NOTIFICATIONS");
        }
    }
}
