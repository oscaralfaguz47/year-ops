using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class oneUpdateLedgerMovementTableType : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
        IF TYPE_ID(N'LedgerMovementType') IS NOT NULL
            DROP TYPE LedgerMovementType;
    ");
            //UPDATE STORED PROCEDURE
            var sp = @"CREATE TYPE LedgerMovementType AS TABLE
(
    IdSeat NVARCHAR(10),
    Consecutive INT,
    Date DATETIME2(7),
    LocalDebit DECIMAL(18,2),
    LocalCredit DECIMAL(18,2),
    AccountingType NVARCHAR(1),
    RecordDate DATETIME2(7),
    AccountingAccountCode NVARCHAR(25),
    CompanyId NVARCHAR(8),
    CostCenterCode NVARCHAR(25)
);";


            // Create new stored Procedure
            migrationBuilder.Sql(sp);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // restart the stored procedure to the original version
            var spOriginal = @"CREATE TYPE LedgerMovementType AS TABLE
        (
            IdSeat NVARCHAR(10),
            Consecutive INT,
            Date DATETIME2(7),
            LocalDebit DECIMAL(18,2),
            LocalCredit DECIMAL(18,2),
            AccountingType NVARCHAR(1),
            RecordDate DATETIME2(7),
            AccountingAccountId INT,
            CompanyId NVARCHAR(8),
            CostCenterId INT
        );";

            migrationBuilder.Sql("DROP TYPE IF LedgerMovementType");
            migrationBuilder.Sql(spOriginal);
        }
    }
}
