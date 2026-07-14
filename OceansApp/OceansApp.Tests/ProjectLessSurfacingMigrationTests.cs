using System.Linq;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using OceansApp.DataAccess.Migrations;
using Xunit;

namespace OceansApp.Tests
{
    /// <summary>
    /// Guards the NHP-2.1 SP change: SP_PAYMENT_SHEETS_GetAllConsultantsToPayWithFilters must
    /// surface project-less consultants who have a non-rejected debit/credit OR reimbursement in
    /// the period (the M1 version only surfaced those with a non-rejected interview). The
    /// surfacing predicate lives in the ProjectLessConsultants CTE; we assert the rebuilt
    /// procedure references both source tables there. The previous (M1) procedure referenced
    /// neither, so this fails until the migration adds them.
    /// </summary>
    public class ProjectLessSurfacingMigrationTests
    {
        private static string GetCreateProcedureSql()
        {
            var migration = new thirtyfiveUpdateSP_PAYMENT_SHEETS_GetAllConsultantsToPayWithFiltersDebitCreditReimbursement();
            // UpOperations builds the operations by invoking Up() with an internal builder.
            return migration.UpOperations
                .OfType<SqlOperation>()
                .Select(o => o.Sql)
                .Last(sql => sql.Contains("CREATE PROCEDURE SP_PAYMENT_SHEETS_GetAllConsultantsToPayWithFilters"));
        }

        [Fact]
        public void Up_SurfacesProjectLessConsultantsWithDebitCredit()
        {
            var sql = GetCreateProcedureSql();
            Assert.Contains("CONSULTANT_PAYMENTS_DEBITS_CREDITS", sql);
        }

        [Fact]
        public void Up_SurfacesProjectLessConsultantsWithReimbursement()
        {
            var sql = GetCreateProcedureSql();
            Assert.Contains("CONSULTANT_REIMBURSED_BENEFITS", sql);
        }

        // Guards the period-disabled fix: a consultant whose only project was removed via the
        // payment-sheet "remove for this period" action must still surface project-less. The fix
        // makes ProjectLessConsultants ignore period-disabled assignments by correlating the
        // disabled-tracking table against AC.ProjectId (unique to this fix). Reverting drops it.
        [Fact]
        public void Up_IgnoresPeriodDisabledAssignmentsForProjectLessSurfacing()
        {
            var migration = new thirtysixFixSP_PAYMENT_SHEETS_ProjectLessDisabledTracking();
            var sql = migration.UpOperations
                .OfType<SqlOperation>()
                .Select(o => o.Sql)
                .Last(s => s.Contains("CREATE PROCEDURE SP_PAYMENT_SHEETS_GetAllConsultantsToPayWithFilters"));
            Assert.Contains("PCPDT.ProjectId = AC.ProjectId", sql);
        }
    }
}
