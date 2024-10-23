using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class createIndexesForImageBlobsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_IMAGE_BLOBS_ContainerName",
                table: "IMAGE_BLOBS",
                column: "ContainerName");

            migrationBuilder.CreateIndex(
                name: "IX_IMAGE_BLOBS_EntityId",
                table: "IMAGE_BLOBS",
                column: "EntityId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_IMAGE_BLOBS_ContainerName",
                table: "IMAGE_BLOBS");

            migrationBuilder.DropIndex(
                name: "IX_IMAGE_BLOBS_EntityId",
                table: "IMAGE_BLOBS");
        }
    }
}
