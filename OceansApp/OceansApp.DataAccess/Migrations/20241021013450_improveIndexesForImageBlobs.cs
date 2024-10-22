using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class improveIndexesForImageBlobs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_IMAGE_BLOBS_EntityId_EntityType",
                table: "IMAGE_BLOBS");

            migrationBuilder.CreateIndex(
                name: "IX_IMAGE_BLOBS_EntityId_EntityType_CreationDate_BlobId",
                table: "IMAGE_BLOBS",
                columns: new[] { "EntityId", "EntityType", "CreationDate", "BlobId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_IMAGE_BLOBS_EntityId_EntityType_CreationDate_BlobId",
                table: "IMAGE_BLOBS");

            migrationBuilder.CreateIndex(
                name: "IX_IMAGE_BLOBS_EntityId_EntityType",
                table: "IMAGE_BLOBS",
                columns: new[] { "EntityId", "EntityType" });
        }
    }
}
