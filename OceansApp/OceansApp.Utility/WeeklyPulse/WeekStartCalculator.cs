namespace OceansApp.Utility.WeeklyPulse
{
    /// <summary>
    /// Shared helper for the Weekly Pulse "week" concept.
    ///
    /// Costa Rica observes UTC-6 year-round (no daylight saving time), so a fixed
    /// offset is used rather than a time-zone database lookup. The "week" begins on
    /// Monday in Costa Rica time. There is no Week table: snapshots and living
    /// entities carry a WeekStart column whose value comes from <see cref="WeekStart"/>.
    /// </summary>
    public static class WeekStartCalculator
    {
        /// <summary>Costa Rica's fixed UTC offset (UTC-6, year-round, no DST).</summary>
        public static readonly TimeSpan CostaRicaUtcOffset = TimeSpan.FromHours(-6);

        /// <summary>
        /// Returns the date of the Monday that begins the Costa Rica week containing
        /// <paramref name="instant"/>. A Sunday belongs to the week that started the
        /// previous Monday.
        /// </summary>
        public static DateOnly WeekStart(DateTimeOffset instant)
        {
            var costaRicaTime = instant.ToOffset(CostaRicaUtcOffset);

            // DayOfWeek: Sunday = 0 .. Saturday = 6. Days elapsed since Monday.
            int daysSinceMonday = ((int)costaRicaTime.DayOfWeek + 6) % 7;

            return DateOnly.FromDateTime(costaRicaTime.Date).AddDays(-daysSinceMonday);
        }

        /// <summary>
        /// Returns the WeekStart for the supplied UTC instant.
        /// </summary>
        public static DateOnly WeekStart(DateTime instantUtc) =>
            WeekStart(new DateTimeOffset(DateTime.SpecifyKind(instantUtc, DateTimeKind.Utc)));

        /// <summary>
        /// The current week's WeekStart, i.e. WeekStart(now).
        /// </summary>
        public static DateOnly Current() => WeekStart(DateTimeOffset.UtcNow);
    }
}
