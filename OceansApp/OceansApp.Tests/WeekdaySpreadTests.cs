using OceansApp.DataAccess.Services;
using Xunit;

namespace OceansApp.Tests
{
    /// <summary>
    /// Tests for the pure weekday-spread helper extracted from AutofillTimeEntryTrackingTool.
    /// Cover: weekdays only; weekends and holidays skipped; period-boundary days included;
    /// an empty period yields no dates. See docs/adr/0002.
    /// </summary>
    public class WeekdaySpreadTests
    {
        [Fact]
        public void GetWeekdayDates_ReturnsWeekdaysOnly_SkippingWeekends()
        {
            // Mon 2026-06-01 .. Sun 2026-06-07 => weekdays Mon..Fri only.
            var start = new DateTime(2026, 6, 1);
            var end = new DateTime(2026, 6, 7);

            var dates = WeekdaySpread.GetWeekdayDates(start, end, holidays: null);

            Assert.Equal(
                new List<DateTime>
                {
                    new(2026, 6, 1),
                    new(2026, 6, 2),
                    new(2026, 6, 3),
                    new(2026, 6, 4),
                    new(2026, 6, 5),
                },
                dates);
            Assert.DoesNotContain(new DateTime(2026, 6, 6), dates); // Saturday
            Assert.DoesNotContain(new DateTime(2026, 6, 7), dates); // Sunday
        }

        [Fact]
        public void GetWeekdayDates_SkipsHolidays()
        {
            // Wed 2026-06-03 is a holiday and must be excluded even though it is a weekday.
            var start = new DateTime(2026, 6, 1);
            var end = new DateTime(2026, 6, 5);
            var holidays = new List<DateTime> { new(2026, 6, 3) };

            var dates = WeekdaySpread.GetWeekdayDates(start, end, holidays);

            Assert.DoesNotContain(new DateTime(2026, 6, 3), dates);
            Assert.Equal(4, dates.Count); // Mon, Tue, Thu, Fri
        }

        [Fact]
        public void GetWeekdayDates_IgnoresHolidayTimeComponent()
        {
            // A holiday supplied with a time-of-day still matches the calendar day.
            var start = new DateTime(2026, 6, 1);
            var end = new DateTime(2026, 6, 5);
            var holidays = new List<DateTime> { new(2026, 6, 4, 13, 30, 0) };

            var dates = WeekdaySpread.GetWeekdayDates(start, end, holidays);

            Assert.DoesNotContain(new DateTime(2026, 6, 4), dates);
        }

        [Fact]
        public void GetWeekdayDates_IncludesPeriodBoundaryDays()
        {
            // Both boundary days are weekdays and must be included.
            var start = new DateTime(2026, 6, 1); // Monday
            var end = new DateTime(2026, 6, 5);   // Friday

            var dates = WeekdaySpread.GetWeekdayDates(start, end, holidays: null);

            Assert.Contains(new DateTime(2026, 6, 1), dates);
            Assert.Contains(new DateTime(2026, 6, 5), dates);
        }

        [Fact]
        public void GetWeekdayDates_IgnoresTimeComponentOnBoundaries()
        {
            // Period boundaries often carry a 23:59 end-of-day time; only the date matters.
            var start = new DateTime(2026, 6, 1, 0, 0, 0);
            var end = new DateTime(2026, 6, 5, 23, 59, 0);

            var dates = WeekdaySpread.GetWeekdayDates(start, end, holidays: null);

            Assert.Equal(5, dates.Count);
            Assert.Contains(new DateTime(2026, 6, 5), dates);
        }

        [Fact]
        public void GetWeekdayDates_EmptyPeriod_YieldsNoDates()
        {
            // End before start => no dates.
            var start = new DateTime(2026, 6, 5);
            var end = new DateTime(2026, 6, 1);

            var dates = WeekdaySpread.GetWeekdayDates(start, end, holidays: null);

            Assert.Empty(dates);
        }

        [Fact]
        public void GetWeekdayDates_WeekendOnlyPeriod_YieldsNoDates()
        {
            // Sat 2026-06-06 .. Sun 2026-06-07 => no weekdays.
            var start = new DateTime(2026, 6, 6);
            var end = new DateTime(2026, 6, 7);

            var dates = WeekdaySpread.GetWeekdayDates(start, end, holidays: null);

            Assert.Empty(dates);
        }
    }
}
