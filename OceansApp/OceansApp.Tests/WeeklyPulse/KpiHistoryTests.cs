using OceansApp.Models.Domain.WeeklyPulse;
using OceansApp.Models.Models;

namespace OceansApp.Tests.WeeklyPulse
{
    /// <summary>
    /// Covers the pure KPI History grouping: a single KPI's weekly results read in
    /// sequence and bucketed by Period (month / quarter / year). The defining rule is that
    /// a Week belongs <b>wholly</b> to the Period containing its Monday (no splitting a Week
    /// across a boundary), and that the per-period <b>tally counts statuses only</b> — free-text
    /// result values are never summed or averaged. No DbContext/HttpContext — operates on
    /// plain <see cref="KpiResult"/> rows.
    /// </summary>
    public class KpiHistoryTests
    {
        private static KpiResult Result(DateOnly week, string value, KpiStatus status) =>
            new() { WeekStart = week, Value = value, Status = status };

        // ---- Week belongs wholly to the Period containing its Monday -------

        [Fact]
        public void WeekStraddlingMonthBoundary_IsGroupedByItsMonday()
        {
            // Monday 2026-06-29 begins a Week that runs into July, but the Week belongs
            // WHOLLY to June — the month containing its Monday — not split across the boundary.
            var results = new[] { Result(new(2026, 6, 29), "92%", KpiStatus.Green) };

            var groups = KpiHistoryService.GroupByPeriod(results, PeriodGranularity.Month);

            var group = Assert.Single(groups);
            Assert.Equal(2026, group.Period.Year);
            Assert.Equal(6, group.Period.Index); // June, not July
        }

        [Fact]
        public void WeekStraddlingQuarterBoundary_IsGroupedByItsMonday()
        {
            // Monday 2026-03-30 begins a Week that runs into April (Q2), but it belongs to
            // Q1 because its Monday falls in March.
            var results = new[] { Result(new(2026, 3, 30), "5 days", KpiStatus.Yellow) };

            var groups = KpiHistoryService.GroupByPeriod(results, PeriodGranularity.Quarter);

            var group = Assert.Single(groups);
            Assert.Equal(2026, group.Period.Year);
            Assert.Equal(1, group.Period.Index); // Q1, not Q2
        }

        [Fact]
        public void WeekStraddlingYearBoundary_IsGroupedByItsMonday()
        {
            // Monday 2025-12-29 begins a Week that runs into January 2026, but it belongs to
            // 2025 because its Monday falls in December.
            var results = new[] { Result(new(2025, 12, 29), "ok", KpiStatus.Green) };

            var groups = KpiHistoryService.GroupByPeriod(results, PeriodGranularity.Year);

            var group = Assert.Single(groups);
            Assert.Equal(2025, group.Period.Year);
        }

        // ---- Grouping by each granularity, read in sequence ----------------

        [Fact]
        public void GroupsWeeklyResultsByMonth_InChronologicalSequence()
        {
            var results = new[]
            {
                Result(new(2026, 6, 8), "b", KpiStatus.Green),  // June
                Result(new(2026, 5, 25), "a", KpiStatus.Green), // May
                Result(new(2026, 6, 1), "c", KpiStatus.Red),    // June
            };

            var groups = KpiHistoryService.GroupByPeriod(results, PeriodGranularity.Month);

            Assert.Equal(2, groups.Count);
            Assert.Equal(5, groups[0].Period.Index); // May first — periods in sequence
            Assert.Equal(6, groups[1].Period.Index);
            // Weeks within a period are themselves in chronological sequence.
            Assert.Equal(new[] { "c", "b" }, groups[1].Results.Select(r => r.Value));
        }

        [Fact]
        public void GroupsByQuarter()
        {
            var results = new[]
            {
                Result(new(2026, 2, 2), "q1", KpiStatus.Green),  // Q1
                Result(new(2026, 8, 3), "q3", KpiStatus.Green),  // Q3
            };

            var groups = KpiHistoryService.GroupByPeriod(results, PeriodGranularity.Quarter);

            Assert.Equal(new[] { 1, 3 }, groups.Select(g => g.Period.Index));
        }

        [Fact]
        public void GroupsByYear()
        {
            var results = new[]
            {
                Result(new(2026, 1, 5), "y", KpiStatus.Green),
                Result(new(2025, 6, 2), "x", KpiStatus.Green),
            };

            var groups = KpiHistoryService.GroupByPeriod(results, PeriodGranularity.Year);

            Assert.Equal(new[] { 2025, 2026 }, groups.Select(g => g.Period.Year));
        }

        // ---- The tally counts statuses; values are never rolled up ---------

        [Fact]
        public void StatusTally_CountsStatuses_NeverSumsOrAveragesValues()
        {
            var results = new[]
            {
                Result(new(2026, 6, 1), "100", KpiStatus.Green),
                Result(new(2026, 6, 8), "200", KpiStatus.Green),
                Result(new(2026, 6, 15), "300", KpiStatus.Red),
            };

            var group = Assert.Single(KpiHistoryService.GroupByPeriod(results, PeriodGranularity.Month));

            // A tally of statuses — never a sum or average of the numeric-looking values.
            Assert.Equal(2, group.StatusTally[KpiStatus.Green]);
            Assert.Equal(1, group.StatusTally[KpiStatus.Red]);
            Assert.False(group.StatusTally.ContainsKey(KpiStatus.Yellow));
            // The free-text values are preserved verbatim, in sequence — not aggregated.
            Assert.Equal(new[] { "100", "200", "300" }, group.Results.Select(r => r.Value));
        }

        [Fact]
        public void Empty_WhenThereAreNoResults()
        {
            Assert.Empty(KpiHistoryService.GroupByPeriod(Array.Empty<KpiResult>(), PeriodGranularity.Month));
        }
    }
}
