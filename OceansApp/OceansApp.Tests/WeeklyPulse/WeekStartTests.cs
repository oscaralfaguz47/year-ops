using OceansApp.Utility.WeeklyPulse;

namespace OceansApp.Tests.WeeklyPulse
{
    public class WeekStartTests
    {
        [Fact]
        public void WeekStart_ReturnsMonday_ForMidWeekInstant()
        {
            // Wednesday 2026-06-24 12:00 UTC -> Wednesday in Costa Rica -> week of Monday 2026-06-22.
            var instant = new DateTimeOffset(2026, 6, 24, 12, 0, 0, TimeSpan.Zero);

            Assert.Equal(new DateOnly(2026, 6, 22), WeekStartCalculator.WeekStart(instant));
        }

        [Fact]
        public void WeekStart_SundayBelongsToPreviousMonday()
        {
            // Sunday 2026-06-28 18:00 Costa Rica time still belongs to the week that began Monday 2026-06-22.
            var instant = new DateTimeOffset(2026, 6, 28, 18, 0, 0, TimeSpan.FromHours(-6));

            Assert.Equal(new DateOnly(2026, 6, 22), WeekStartCalculator.WeekStart(instant));
        }

        [Fact]
        public void WeekStart_UsesCostaRicaOffset_NotUtc()
        {
            // 2026-06-29 03:00 UTC is Monday in UTC, but 2026-06-28 21:00 in Costa Rica (UTC-6),
            // which is still Sunday -> week of the previous Monday 2026-06-22, not 2026-06-29.
            var instant = new DateTimeOffset(2026, 6, 29, 3, 0, 0, TimeSpan.Zero);

            Assert.Equal(new DateOnly(2026, 6, 22), WeekStartCalculator.WeekStart(instant));
        }

        [Fact]
        public void WeekStart_ReturnsSameMonday_AtStartOfMonday()
        {
            // Monday 2026-06-29 00:00 Costa Rica time is the start of its own week.
            var instant = new DateTimeOffset(2026, 6, 29, 0, 0, 0, TimeSpan.FromHours(-6));

            Assert.Equal(new DateOnly(2026, 6, 29), WeekStartCalculator.WeekStart(instant));
        }
    }
}
