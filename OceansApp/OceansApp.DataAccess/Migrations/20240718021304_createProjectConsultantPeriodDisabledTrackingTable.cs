using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class createProjectConsultantPeriodDisabledTrackingTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProjectConsultantPeriodDisabledTracking",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjectId = table.Column<int>(type: "int", nullable: false),
                    ConsultantId = table.Column<int>(type: "int", nullable: false),
                    StartPeriodDate = table.Column<DateTime>(type: "date", nullable: false),
                    EndPeriodDate = table.Column<DateTime>(type: "date", nullable: false),
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectConsultantPeriodDisabledTracking", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectConsultantPeriodDisabledTracking_CONSULTANT_DETAILS_ConsultantId",
                        column: x => x.ConsultantId,
                        principalTable: "CONSULTANT_DETAILS",
                        principalColumn: "ConsultantId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProjectConsultantPeriodDisabledTracking_PROJECTS_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "PROJECTS",
                        principalColumn: "ProjectId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProjectConsultantPeriodDisabledTracking_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProjectConsultantPeriodDisabledTracking_ConsultantId",
                table: "ProjectConsultantPeriodDisabledTracking",
                column: "ConsultantId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectConsultantPeriodDisabledTracking_ConsultantId_ProjectId_StartPeriodDate_EndPeriodDate",
                table: "ProjectConsultantPeriodDisabledTracking",
                columns: new[] { "ConsultantId", "ProjectId", "StartPeriodDate", "EndPeriodDate" });

            migrationBuilder.CreateIndex(
                name: "IX_ProjectConsultantPeriodDisabledTracking_CreatedBy",
                table: "ProjectConsultantPeriodDisabledTracking",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectConsultantPeriodDisabledTracking_EndPeriodDate",
                table: "ProjectConsultantPeriodDisabledTracking",
                column: "EndPeriodDate");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectConsultantPeriodDisabledTracking_ProjectId",
                table: "ProjectConsultantPeriodDisabledTracking",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectConsultantPeriodDisabledTracking_ProjectId_ConsultantId",
                table: "ProjectConsultantPeriodDisabledTracking",
                columns: new[] { "ProjectId", "ConsultantId" });

            migrationBuilder.CreateIndex(
                name: "IX_ProjectConsultantPeriodDisabledTracking_StartPeriodDate",
                table: "ProjectConsultantPeriodDisabledTracking",
                column: "StartPeriodDate");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProjectConsultantPeriodDisabledTracking");
        }
    }
}
