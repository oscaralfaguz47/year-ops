using OceansApp.DataAccess.Services;
using OceansApp.Models.ViewModels.PaymentAnchor;
using Xunit;

namespace OceansApp.Tests
{
    public class PaymentAnchorResolverTests
    {
        private static readonly DateTime PeriodEnd = new(2026, 6, 30);

        [Fact]
        public void ResolveFromHistory_MostRecentAssignmentWins()
        {
            var history = new List<PaymentAnchorHistoryRecord>
            {
                // Older assignment on project 10.
                new() { ProjectConsultantAssignedId = 1, ProjectId = 10, PositionId = 100,
                        HourlySalary = 20m, ActionDate = new DateTime(2026, 1, 15), Id = 1 },
                // More-recent assignment on project 20 — this one should win.
                new() { ProjectConsultantAssignedId = 2, ProjectId = 20, PositionId = 200,
                        HourlySalary = 30m, ActionDate = new DateTime(2026, 4, 10), Id = 2 },
            };

            var anchor = PaymentAnchorResolver.ResolveFromHistory(history, PeriodEnd);

            Assert.NotNull(anchor);
            Assert.Equal(20, anchor!.ProjectId);
            Assert.Equal(200, anchor.PositionId);
            Assert.Equal(30m, anchor.Rate);
        }

        [Fact]
        public void ResolveFromHistory_RateComesFromMostRecentRecordWithSalary_ZeroedRowIgnored()
        {
            var history = new List<PaymentAnchorHistoryRecord>
            {
                // Salaried assignment...
                new() { ProjectConsultantAssignedId = 5, ProjectId = 50, PositionId = 500,
                        HourlySalary = 42m, ActionDate = new DateTime(2026, 2, 1), Id = 1 },
                // ...later unassigned, which zeroes the salary. This row must NOT set the rate,
                // but the assignment is still the anchor (project/position resolve from it).
                new() { ProjectConsultantAssignedId = 5, ProjectId = 50, PositionId = 500,
                        HourlySalary = 0m, ActionDate = new DateTime(2026, 5, 1), Id = 2 },
            };

            var anchor = PaymentAnchorResolver.ResolveFromHistory(history, PeriodEnd);

            Assert.NotNull(anchor);
            Assert.Equal(50, anchor!.ProjectId);
            Assert.Equal(500, anchor.PositionId);
            Assert.Equal(42m, anchor.Rate);
        }

        [Fact]
        public void ResolveFromHistory_ReturnsNull_WhenNoAssignmentHistory()
        {
            var anchor = PaymentAnchorResolver.ResolveFromHistory(
                new List<PaymentAnchorHistoryRecord>(), PeriodEnd);

            Assert.Null(anchor);
        }

        [Fact]
        public void ResolveFromHistory_ReturnsNull_WhenAllHistoryIsAfterPeriodEnd()
        {
            var history = new List<PaymentAnchorHistoryRecord>
            {
                new() { ProjectConsultantAssignedId = 9, ProjectId = 90, PositionId = 900,
                        HourlySalary = 10m, ActionDate = new DateTime(2026, 7, 15), Id = 1 },
            };

            var anchor = PaymentAnchorResolver.ResolveFromHistory(history, PeriodEnd);

            Assert.Null(anchor);
        }
    }
}
