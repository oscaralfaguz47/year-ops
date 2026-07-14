namespace OceansApp.DataAccess.Services
{
    /// <summary>
    /// Pure spread helper for the admin Manual Hours Upload: turns a period <em>total</em> into a
    /// per-workable-day quantity. The admin enters the total they are filing; this splits it evenly
    /// across the period's workable days (<see cref="WeekdaySpread"/>) to the cent, with the last few
    /// days carrying one extra cent each to soak up the remainder so the filed total equals the entered
    /// total exactly. See docs/adr/0003.
    ///
    /// Kept pure (no DB, no clock) so the spread/remainder maths — the riskiest part of the feature —
    /// is unit-testable in isolation, mirroring <see cref="WeekdaySpread"/>.
    /// </summary>
    public static class PeriodTotalSpread
    {
        /// <summary>
        /// Spread <paramref name="totalHours"/> across <paramref name="workableDays"/> days. Returns a
        /// list of length <paramref name="workableDays"/>, one quantity per day in order, that sums to
        /// exactly <paramref name="totalHours"/>. The split is even to the cent: every day gets the base
        /// cents-per-day and the <em>last</em> few days carry one extra cent each to soak up the
        /// remainder — so no day can ever go negative and entered total == filed total. Days that come
        /// out at <c>0</c> (a tiny total, fewer cents than days) stay <c>0</c> for the caller to skip,
        /// with the non-zero cents landing on the last days. In the default case <c>total = days × 8</c>
        /// this is a flat <c>8.00</c>/day with no remainder.
        /// </summary>
        /// <remarks>
        /// Works in integer cents rather than folding a single decimal remainder onto one day: that
        /// simpler scheme can drive the last day negative when the per-day rounds up (e.g. 0.5h over
        /// 14 days), which would corrupt a pay figure. See docs/adr/0003.
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="workableDays"/> is not positive, or <paramref name="totalHours"/> is not
        /// positive, or <paramref name="totalHours"/> is positive but rounds to zero cents (too small to file).
        /// </exception>
        public static List<decimal> Spread(decimal totalHours, int workableDays)
        {
            if (workableDays <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(workableDays), "There must be at least one workable day.");
            }
            if (totalHours <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(totalHours), "The total hours must be greater than zero.");
            }

            var totalCents = (long)Math.Round(totalHours * 100m, MidpointRounding.AwayFromZero);
            if (totalCents == 0)
            {
                // A positive total smaller than half a cent (e.g. 0.004h) rounds to zero cents, which
                // would spread to all-zero days — the caller skips every day and files an EMPTY
                // submission. Reject here so "entered total == filed total" can't degrade to 0. See docs/adr/0003.
                throw new ArgumentOutOfRangeException(nameof(totalHours), "The total hours are too small to file to the cent.");
            }
            var baseCents = totalCents / workableDays;
            var extraDays = (int)(totalCents % workableDays); // this many trailing days get one extra cent

            var quantities = new List<decimal>(workableDays);
            for (var i = 0; i < workableDays; i++)
            {
                var cents = baseCents + (i >= workableDays - extraDays ? 1 : 0);
                quantities.Add(cents / 100m);
            }

            return quantities;
        }
    }
}
