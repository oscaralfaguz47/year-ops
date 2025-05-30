using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class createIndexesToImproveQueryForGetBillableHours : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_Id",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_CONSULTANT_DETAILS_ConsultantId",
                table: "CONSULTANT_DETAILS");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Id",
                table: "Users",
                column: "Id")
                .Annotation("SqlServer:Include", new[] { "Name", "LastName" });

            migrationBuilder.CreateIndex(
                name: "IX_REPORTING_MY_TIME_MOVEMENTS_ProjectId_ConsultantId_ActionDate_IsBillable_TransactionStatusId",
                table: "REPORTING_MY_TIME_MOVEMENTS",
                columns: new[] { "ProjectId", "ConsultantId", "ActionDate", "IsBillable", "TransactionStatusId" });

            migrationBuilder.CreateIndex(
                name: "IX_PROJECTS_ProjectId_ClientId_SuccessManagerId",
                table: "PROJECTS",
                columns: new[] { "ProjectId", "ClientId", "SuccessManagerId" });

            migrationBuilder.CreateIndex(
                name: "IX_PCC_ConfigMatch",
                table: "PRODUCTS_CLIENTS_COMPANIES_ACCOUNTING_CONFIG",
                columns: new[] { "ClientId", "CompanyId", "MovementTypeId" })
                .Annotation("SqlServer:Include", new[] { "ProductId", "TaxPercentage" });

            migrationBuilder.CreateIndex(
                name: "IX_PRODUCTS_Code",
                table: "PRODUCTS",
                column: "ProductCode")
                .Annotation("SqlServer:Include", new[] { "ProductId" });

            migrationBuilder.CreateIndex(
                name: "IX_CD_ConsultantId",
                table: "CONSULTANT_DETAILS",
                column: "ConsultantId")
                .Annotation("SqlServer:Include", new[] { "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_CLIENT_KeyColumns",
                table: "CLIENT",
                column: "ClientId")
                .Annotation("SqlServer:Include", new[] { "CompanyId", "LimitNumHoursForOverTime", "OverTimeAmount" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_Id",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_REPORTING_MY_TIME_MOVEMENTS_ProjectId_ConsultantId_ActionDate_IsBillable_TransactionStatusId",
                table: "REPORTING_MY_TIME_MOVEMENTS");

            migrationBuilder.DropIndex(
                name: "IX_PROJECTS_ProjectId_ClientId_SuccessManagerId",
                table: "PROJECTS");

            migrationBuilder.DropIndex(
                name: "IX_PCC_ConfigMatch",
                table: "PRODUCTS_CLIENTS_COMPANIES_ACCOUNTING_CONFIG");

            migrationBuilder.DropIndex(
                name: "IX_PRODUCTS_Code",
                table: "PRODUCTS");

            migrationBuilder.DropIndex(
                name: "IX_CD_ConsultantId",
                table: "CONSULTANT_DETAILS");

            migrationBuilder.DropIndex(
                name: "IX_CLIENT_KeyColumns",
                table: "CLIENT");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Id",
                table: "Users",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_CONSULTANT_DETAILS_ConsultantId",
                table: "CONSULTANT_DETAILS",
                column: "ConsultantId");
        }
    }
}
