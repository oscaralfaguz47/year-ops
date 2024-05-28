using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class createReportingMyTimeMovementBlobsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "REPORTING_MY_TIME_MOVEMENT_BLOBS",
                columns: table => new
                {
                    InternalBlobId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MovementId = table.Column<int>(type: "int", nullable: false),
                    BlobName = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: false),
                    ContainerId = table.Column<string>(type: "VARCHAR(255)", maxLength: 255, nullable: false),
                    BlobUrl = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Size = table.Column<long>(type: "bigint", nullable: false),
                    ContentType = table.Column<string>(type: "VARCHAR(255)", maxLength: 255, nullable: false),
                    Metadata = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_REPORTING_MY_TIME_MOVEMENT_BLOBS", x => x.InternalBlobId);
                    table.ForeignKey(
                        name: "FK_REPORTING_MY_TIME_MOVEMENT_BLOBS_REPORTING_MY_TIME_MOVEMENTS_MovementId",
                        column: x => x.MovementId,
                        principalTable: "REPORTING_MY_TIME_MOVEMENTS",
                        principalColumn: "MovementId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_REPORTING_MY_TIME_MOVEMENT_BLOBS_MovementId",
                table: "REPORTING_MY_TIME_MOVEMENT_BLOBS",
                column: "MovementId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "REPORTING_MY_TIME_MOVEMENT_BLOBS");
        }
    }
}
