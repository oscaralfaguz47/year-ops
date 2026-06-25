using OceansApp.Models.ViewModels.PaymentAnchor;
using OceansApp.Utility.SharedMethods;

namespace OceansApp.DataAccess.Services
{
    /// <summary>
    /// Payment Anchor Resolver — given a consultant's assignment history and a period end,
    /// resolves the most-recent historical assignment (active or not) that should anchor
    /// non-hours payments for a project-less consultant. See docs/adr/0001.
    ///
    /// The resolution rule is implemented as a pure function (<see cref="ResolveFromHistory"/>)
    /// so it can be unit tested in isolation; <see cref="IPaymentAnchorResolver"/> wraps it with
    /// the database fetch.
    /// </summary>
    public static class PaymentAnchorResolver
    {
        /// <summary>
        /// Resolve the payment anchor from a flat list of assignment-history records.
        /// </summary>
        /// <returns>
        /// The anchor (ProjectId, PositionId, Rate) taken from the most-recent assignment at or
        /// before <paramref name="periodEnd"/>, with the Rate coming from the most-recent record
        /// of that assignment whose salary is &gt; 0; <c>null</c> when there is no history at all
        /// at or before the period end.
        /// </returns>
        public static PaymentAnchorResult? ResolveFromHistory(
            IEnumerable<PaymentAnchorHistoryRecord> history, DateTime periodEnd)
        {
            if (history == null)
            {
                return null;
            }

            var eligible = history
                .Where(r => r.ActionDate.Date <= periodEnd.Date)
                .ToList();

            if (eligible.Count == 0)
            {
                // No assignment history => no anchor (consultant who joined purely for interviews).
                return null;
            }

            // The most-recent assignment wins, regardless of whether it is currently active.
            var mostRecentRecord = eligible
                .OrderByDescending(r => r.ActionDate)
                .ThenByDescending(r => r.Id)
                .First();

            var anchorAssignmentRecords = eligible
                .Where(r => r.ProjectConsultantAssignedId == mostRecentRecord.ProjectConsultantAssignedId)
                .ToList();

            // Rate comes from the most-recent record of the anchor assignment WITH salary > 0,
            // so an unassignment / zeroed-salary row does not wipe out the rate.
            var rateRecord = anchorAssignmentRecords
                .Where(r => (r.HourlySalary ?? 0m) > 0m || (r.MonthlySalary ?? 0m) > 0m)
                .OrderByDescending(r => r.ActionDate)
                .ThenByDescending(r => r.Id)
                .FirstOrDefault();

            return new PaymentAnchorResult
            {
                ProjectId = mostRecentRecord.ProjectId,
                PositionId = mostRecentRecord.PositionId,
                Rate = ToHourlyRate(rateRecord, periodEnd)
            };
        }

        private static decimal ToHourlyRate(PaymentAnchorHistoryRecord? rateRecord, DateTime periodEnd)
        {
            if (rateRecord == null)
            {
                return 0m;
            }

            if ((rateRecord.HourlySalary ?? 0m) > 0m)
            {
                return rateRecord.HourlySalary!.Value;
            }

            var monthly = rateRecord.MonthlySalary ?? 0m;
            if (monthly <= 0m)
            {
                return 0m;
            }

            // Convert a monthly salary to an hourly-equivalent the same way the Payment Sheet does.
            var workingDays = DateAndTimes.GetWorkingDaysInMonth(periodEnd);
            if (workingDays <= 0)
            {
                return 0m;
            }

            return (monthly / workingDays) / 8m;
        }
    }
}
