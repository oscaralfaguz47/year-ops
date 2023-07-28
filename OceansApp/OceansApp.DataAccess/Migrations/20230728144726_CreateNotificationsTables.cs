using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    public partial class CreateNotificationsTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NOTIFICATION_MEDIA",
                columns: table => new
                {
                    NotificationMediaId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NOTIFICATION_MEDIA", x => x.NotificationMediaId);
                });

            migrationBuilder.CreateTable(
                name: "NOTIFICATION_STATUS",
                columns: table => new
                {
                    NotificationStatusId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NOTIFICATION_STATUS", x => x.NotificationStatusId);
                });

            migrationBuilder.CreateTable(
                name: "NOTIFICATION_TYPES",
                columns: table => new
                {
                    NotificationTypeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NOTIFICATION_TYPES", x => x.NotificationTypeId);
                });

            migrationBuilder.CreateTable(
                name: "NOTIFICATIONS",
                columns: table => new
                {
                    NotificationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NotificationTypeId = table.Column<int>(type: "int", nullable: false),
                    Body = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Subject = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Remitent = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    SentDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NOTIFICATIONS", x => x.NotificationId);
                    table.ForeignKey(
                        name: "FK_NOTIFICATIONS_NOTIFICATION_TYPES_NotificationTypeId",
                        column: x => x.NotificationTypeId,
                        principalTable: "NOTIFICATION_TYPES",
                        principalColumn: "NotificationTypeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NOTIFICATION_RECIPIENTS",
                columns: table => new
                {
                    RecipientId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RecipientMediaInfo = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    NotificationId = table.Column<int>(type: "int", nullable: false),
                    NotificationMediaId = table.Column<int>(type: "int", nullable: false),
                    NotificationStatusId = table.Column<int>(type: "int", nullable: false),
                    RecipientUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NOTIFICATION_RECIPIENTS", x => x.RecipientId);
                    table.ForeignKey(
                        name: "FK_NOTIFICATION_RECIPIENTS_NOTIFICATION_MEDIA_NotificationMediaId",
                        column: x => x.NotificationMediaId,
                        principalTable: "NOTIFICATION_MEDIA",
                        principalColumn: "NotificationMediaId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NOTIFICATION_RECIPIENTS_NOTIFICATION_STATUS_NotificationStatusId",
                        column: x => x.NotificationStatusId,
                        principalTable: "NOTIFICATION_STATUS",
                        principalColumn: "NotificationStatusId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NOTIFICATION_RECIPIENTS_NOTIFICATIONS_NotificationId",
                        column: x => x.NotificationId,
                        principalTable: "NOTIFICATIONS",
                        principalColumn: "NotificationId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NOTIFICATION_RECIPIENTS_Users_RecipientUserId",
                        column: x => x.RecipientUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_NOTIFICATION_RECIPIENTS_NotificationId",
                table: "NOTIFICATION_RECIPIENTS",
                column: "NotificationId");

            migrationBuilder.CreateIndex(
                name: "IX_NOTIFICATION_RECIPIENTS_NotificationMediaId",
                table: "NOTIFICATION_RECIPIENTS",
                column: "NotificationMediaId");

            migrationBuilder.CreateIndex(
                name: "IX_NOTIFICATION_RECIPIENTS_NotificationStatusId",
                table: "NOTIFICATION_RECIPIENTS",
                column: "NotificationStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_NOTIFICATION_RECIPIENTS_RecipientUserId",
                table: "NOTIFICATION_RECIPIENTS",
                column: "RecipientUserId");

            migrationBuilder.CreateIndex(
                name: "IX_NOTIFICATIONS_NotificationTypeId",
                table: "NOTIFICATIONS",
                column: "NotificationTypeId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NOTIFICATION_RECIPIENTS");

            migrationBuilder.DropTable(
                name: "NOTIFICATION_MEDIA");

            migrationBuilder.DropTable(
                name: "NOTIFICATION_STATUS");

            migrationBuilder.DropTable(
                name: "NOTIFICATIONS");

            migrationBuilder.DropTable(
                name: "NOTIFICATION_TYPES");
        }
    }
}
