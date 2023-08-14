using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    public partial class createDocumentsCCNotificationsTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DOCUMENTS_CC_NOTIFICATIONS",
                columns: table => new
                {
                    DocumentCCId = table.Column<int>(type: "int", nullable: false),
                    NotificationId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DOCUMENTS_CC_NOTIFICATIONS", x => new { x.DocumentCCId, x.NotificationId });
                    table.ForeignKey(
                        name: "FK_DOCUMENTS_CC_NOTIFICATIONS_DOCUMENTS_CC_DocumentCCId",
                        column: x => x.DocumentCCId,
                        principalTable: "DOCUMENTS_CC",
                        principalColumn: "DocumentCCId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DOCUMENTS_CC_NOTIFICATIONS_NOTIFICATIONS_NotificationId",
                        column: x => x.NotificationId,
                        principalTable: "NOTIFICATIONS",
                        principalColumn: "NotificationId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DOCUMENTS_CC_NOTIFICATIONS_NotificationId",
                table: "DOCUMENTS_CC_NOTIFICATIONS",
                column: "NotificationId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DOCUMENTS_CC_NOTIFICATIONS");
        }
    }
}
