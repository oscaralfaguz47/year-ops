namespace OceansApp.DataAccess.Services
{
    /// <summary>
    /// Weekday-spread helper — the pure date logic extracted from
    /// <c>AutofillTimeEntryTrackingTool</c>. Given a pay-period range and the period's holidays,
    /// it returns the calendar days hours should be populated on: every weekday in the range,
    /// skipping weekends and holidays. See docs/adr/0002.
    ///
    /// Kept as a pure function (no DB, no time-of-day, no <c>DateTime.Now</c>) so both the
    /// consultant's own autofill and the admin on-behalf upload share one spread implementation
    /// and it can be unit tested in isolation (mirrors how the Payment Anchor Resolver was isolated).
    /// </summary>
    public static class WeekdaySpread
    {
        /// <summary>
        /// Return the weekday dates in <c>[periodStart, periodEnd]</c> (inclusive, date-only),
        /// skipping Saturdays, Sundays and any supplied holidays. Boundary days are included when
        /// they are weekdays. An empty or inverted period yields an empty list.
        /// </summary>
        public static List<DateTime> GetWeekdayDates(DateTime periodStart, DateTime periodEnd,
            IEnumerable<DateTime>? holidays)
        {
            var holidayDays = holidays == null
                ? new HashSet<DateTime>()
                : holidays.Select(h => h.Date).ToHashSet();

            var result = new List<DateTime>();
            for (var date = periodStart.Date; date <= periodEnd.Date; date = date.AddDays(1))
            {
                if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
                {
                    continue;
                }

                if (holidayDays.Contains(date))
                {
                    continue;
                }

                result.Add(date);
            }

            return result;
        }
    }
}
