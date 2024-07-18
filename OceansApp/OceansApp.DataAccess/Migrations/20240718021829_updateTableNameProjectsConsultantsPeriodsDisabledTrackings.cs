using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class updateTableNameProjectsConsultantsPeriodsDisabledTrackings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProjectConsultantPeriodDisabledTracking_CONSULTANT_DETAILS_ConsultantId",
                table: "ProjectConsultantPeriodDisabledTracking");

            migrationBuilder.DropForeignKey(
                name: "FK_ProjectConsultantPeriodDisabledTracking_PROJECTS_ProjectId",
                table: "ProjectConsultantPeriodDisabledTracking");

            migrationBuilder.DropForeignKey(
                name: "FK_ProjectConsultantPeriodDisabledTracking_Users_CreatedBy",
                table: "ProjectConsultantPeriodDisabledTracking");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProjectConsultantPeriodDisabledTracking",
                table: "ProjectConsultantPeriodDisabledTracking");

            migrationBuilder.RenameTable(
                name: "ProjectConsultantPeriodDisabledTracking",
                newName: "PROJECTS_CONSULTANTS_PERIODS_DISABLED_TRACKINGS");

            migrationBuilder.RenameIndex(
                name: "IX_ProjectConsultantPeriodDisabledTracking_StartPeriodDate",
                table: "PROJECTS_CONSULTANTS_PERIODS_DISABLED_TRACKINGS",
                newName: "IX_PROJECTS_CONSULTANTS_PERIODS_DISABLED_TRACKINGS_StartPeriodDate");

            migrationBuilder.RenameIndex(
                name: "IX_ProjectConsultantPeriodDisabledTracking_ProjectId_ConsultantId",
                table: "PROJECTS_CONSULTANTS_PERIODS_DISABLED_TRACKINGS",
                newName: "IX_PROJECTS_CONSULTANTS_PERIODS_DISABLED_TRACKINGS_ProjectId_ConsultantId");

            migrationBuilder.RenameIndex(
                name: "IX_ProjectConsultantPeriodDisabledTracking_ProjectId",
                table: "PROJECTS_CONSULTANTS_PERIODS_DISABLED_TRACKINGS",
                newName: "IX_PROJECTS_CONSULTANTS_PERIODS_DISABLED_TRACKINGS_ProjectId");

            migrationBuilder.RenameIndex(
                name: "IX_ProjectConsultantPeriodDisabledTracking_EndPeriodDate",
                table: "PROJECTS_CONSULTANTS_PERIODS_DISABLED_TRACKINGS",
                newName: "IX_PROJECTS_CONSULTANTS_PERIODS_DISABLED_TRACKINGS_EndPeriodDate");

            migrationBuilder.RenameIndex(
                name: "IX_ProjectConsultantPeriodDisabledTracking_CreatedBy",
                table: "PROJECTS_CONSULTANTS_PERIODS_DISABLED_TRACKINGS",
                newName: "IX_PROJECTS_CONSULTANTS_PERIODS_DISABLED_TRACKINGS_CreatedBy");

            migrationBuilder.RenameIndex(
                name: "IX_ProjectConsultantPeriodDisabledTracking_ConsultantId_ProjectId_StartPeriodDate_EndPeriodDate",
                table: "PROJECTS_CONSULTANTS_PERIODS_DISABLED_TRACKINGS",
                newName: "IX_PROJECTS_CONSULTANTS_PERIODS_DISABLED_TRACKINGS_ConsultantId_ProjectId_StartPeriodDate_EndPeriodDate");

            migrationBuilder.RenameIndex(
                name: "IX_ProjectConsultantPeriodDisabledTracking_ConsultantId",
                table: "PROJECTS_CONSULTANTS_PERIODS_DISABLED_TRACKINGS",
                newName: "IX_PROJECTS_CONSULTANTS_PERIODS_DISABLED_TRACKINGS_ConsultantId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PROJECTS_CONSULTANTS_PERIODS_DISABLED_TRACKINGS",
                table: "PROJECTS_CONSULTANTS_PERIODS_DISABLED_TRACKINGS",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PROJECTS_CONSULTANTS_PERIODS_DISABLED_TRACKINGS_CONSULTANT_DETAILS_ConsultantId",
                table: "PROJECTS_CONSULTANTS_PERIODS_DISABLED_TRACKINGS",
                column: "ConsultantId",
                principalTable: "CONSULTANT_DETAILS",
                principalColumn: "ConsultantId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PROJECTS_CONSULTANTS_PERIODS_DISABLED_TRACKINGS_PROJECTS_ProjectId",
                table: "PROJECTS_CONSULTANTS_PERIODS_DISABLED_TRACKINGS",
                column: "ProjectId",
                principalTable: "PROJECTS",
                principalColumn: "ProjectId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PROJECTS_CONSULTANTS_PERIODS_DISABLED_TRACKINGS_Users_CreatedBy",
                table: "PROJECTS_CONSULTANTS_PERIODS_DISABLED_TRACKINGS",
                column: "CreatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PROJECTS_CONSULTANTS_PERIODS_DISABLED_TRACKINGS_CONSULTANT_DETAILS_ConsultantId",
                table: "PROJECTS_CONSULTANTS_PERIODS_DISABLED_TRACKINGS");

            migrationBuilder.DropForeignKey(
                name: "FK_PROJECTS_CONSULTANTS_PERIODS_DISABLED_TRACKINGS_PROJECTS_ProjectId",
                table: "PROJECTS_CONSULTANTS_PERIODS_DISABLED_TRACKINGS");

            migrationBuilder.DropForeignKey(
                name: "FK_PROJECTS_CONSULTANTS_PERIODS_DISABLED_TRACKINGS_Users_CreatedBy",
                table: "PROJECTS_CONSULTANTS_PERIODS_DISABLED_TRACKINGS");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PROJECTS_CONSULTANTS_PERIODS_DISABLED_TRACKINGS",
                table: "PROJECTS_CONSULTANTS_PERIODS_DISABLED_TRACKINGS");

            migrationBuilder.RenameTable(
                name: "PROJECTS_CONSULTANTS_PERIODS_DISABLED_TRACKINGS",
                newName: "ProjectConsultantPeriodDisabledTracking");

            migrationBuilder.RenameIndex(
                name: "IX_PROJECTS_CONSULTANTS_PERIODS_DISABLED_TRACKINGS_StartPeriodDate",
                table: "ProjectConsultantPeriodDisabledTracking",
                newName: "IX_ProjectConsultantPeriodDisabledTracking_StartPeriodDate");

            migrationBuilder.RenameIndex(
                name: "IX_PROJECTS_CONSULTANTS_PERIODS_DISABLED_TRACKINGS_ProjectId_ConsultantId",
                table: "ProjectConsultantPeriodDisabledTracking",
                newName: "IX_ProjectConsultantPeriodDisabledTracking_ProjectId_ConsultantId");

            migrationBuilder.RenameIndex(
                name: "IX_PROJECTS_CONSULTANTS_PERIODS_DISABLED_TRACKINGS_ProjectId",
                table: "ProjectConsultantPeriodDisabledTracking",
                newName: "IX_ProjectConsultantPeriodDisabledTracking_ProjectId");

            migrationBuilder.RenameIndex(
                name: "IX_PROJECTS_CONSULTANTS_PERIODS_DISABLED_TRACKINGS_EndPeriodDate",
                table: "ProjectConsultantPeriodDisabledTracking",
                newName: "IX_ProjectConsultantPeriodDisabledTracking_EndPeriodDate");

            migrationBuilder.RenameIndex(
                name: "IX_PROJECTS_CONSULTANTS_PERIODS_DISABLED_TRACKINGS_CreatedBy",
                table: "ProjectConsultantPeriodDisabledTracking",
                newName: "IX_ProjectConsultantPeriodDisabledTracking_CreatedBy");

            migrationBuilder.RenameIndex(
                name: "IX_PROJECTS_CONSULTANTS_PERIODS_DISABLED_TRACKINGS_ConsultantId_ProjectId_StartPeriodDate_EndPeriodDate",
                table: "ProjectConsultantPeriodDisabledTracking",
                newName: "IX_ProjectConsultantPeriodDisabledTracking_ConsultantId_ProjectId_StartPeriodDate_EndPeriodDate");

            migrationBuilder.RenameIndex(
                name: "IX_PROJECTS_CONSULTANTS_PERIODS_DISABLED_TRACKINGS_ConsultantId",
                table: "ProjectConsultantPeriodDisabledTracking",
                newName: "IX_ProjectConsultantPeriodDisabledTracking_ConsultantId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProjectConsultantPeriodDisabledTracking",
                table: "ProjectConsultantPeriodDisabledTracking",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectConsultantPeriodDisabledTracking_CONSULTANT_DETAILS_ConsultantId",
                table: "ProjectConsultantPeriodDisabledTracking",
                column: "ConsultantId",
                principalTable: "CONSULTANT_DETAILS",
                principalColumn: "ConsultantId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectConsultantPeriodDisabledTracking_PROJECTS_ProjectId",
                table: "ProjectConsultantPeriodDisabledTracking",
                column: "ProjectId",
                principalTable: "PROJECTS",
                principalColumn: "ProjectId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectConsultantPeriodDisabledTracking_Users_CreatedBy",
                table: "ProjectConsultantPeriodDisabledTracking",
                column: "CreatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
