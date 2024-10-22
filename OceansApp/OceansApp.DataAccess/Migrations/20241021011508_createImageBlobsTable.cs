using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class createImageBlobsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "IMAGE_BLOBS",
                columns: table => new
                {
                    BlobId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BlobName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ContainerName = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    BlobUrl = table.Column<string>(type: "varchar(MAX)", nullable: false),
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EntityId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    EntityType = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IMAGE_BLOBS", x => x.BlobId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_IMAGE_BLOBS_BlobName",
                table: "IMAGE_BLOBS",
                column: "BlobName");

            migrationBuilder.CreateIndex(
                name: "IX_IMAGE_BLOBS_ContainerName_BlobName",
                table: "IMAGE_BLOBS",
                columns: new[] { "ContainerName", "BlobName" });

            migrationBuilder.CreateIndex(
                name: "IX_IMAGE_BLOBS_CreationDate",
                table: "IMAGE_BLOBS",
                column: "CreationDate");

            migrationBuilder.CreateIndex(
                name: "IX_IMAGE_BLOBS_EntityId_EntityType",
                table: "IMAGE_BLOBS",
                columns: new[] { "EntityId", "EntityType" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IMAGE_BLOBS");
        }
    }
}
