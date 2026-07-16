using OceansApp.Models.Models;

namespace OceansApp.Models.Domain.WeeklyPulse
{
    /// <summary>
    /// Identifies one Period bucket for KPI History. <see cref="Index"/> is the month
    /// (1–12) when <see cref="Granularity"/> is <see cref="PeriodGranularity.Month"/>, the
    /// quarter (1–4) when Quarter, and 0 (unused) when Year. Two Periods are equal iff their
    /// granularity, year and index match — that equality is what buckets the Weeks.
    /// </summary>
    public readonly record struct KpiPeriod(PeriodGranularity Granularity, int Year, int Index);

    /// <summary>
    /// One Period's slice of a single KPI's history: the Period it covers, the weekly
    /// <see cref="KpiResult"/> rows that fall in it (read in chronological sequence), and a
    /// <b>tally of statuses</b>. The tally only <i>counts</i> how many Weeks were each
    /// Green/Yellow/Red — it never sums or averages the free-text result values.
    /// </summary>
    public class KpiPeriodGroup
    {
        public KpiPeriod Period { get; init; }

        /// <summary>The Week results in this Period, in chronological (Monday-ascending) order.</summary>
        public IReadOnlyList<KpiResult> Results { get; init; } = new List<KpiResult>();

        /// <summary>
        /// How many Weeks in this Period carried each <see cref="KpiStatus"/> — a count only,
        /// never an arithmetic roll-up of the values. Statuses with no Weeks are absent.
        /// </summary>
        public IReadOnlyDictionary<KpiStatus, int> StatusTally { get; init; } =
            new Dictionary<KpiStatus, int>();
    }

    /// <summary>
    /// Pure domain logic for the Weekly Pulse KPI History — a read-only lens on a SINGLE
    /// KPI's weekly results read in sequence and grouped by Period (month / quarter / year).
    ///
    /// <para>The defining rule: a Week belongs <b>wholly</b> to the Period containing its
    /// Monday (its <see cref="KpiResult.WeekStart"/>), never split across a month/quarter/year
    /// boundary. This is <i>display only</i> — there is no arithmetic roll-up of the free-text
    /// result values; the per-Period <see cref="KpiPeriodGroup.StatusTally"/> may count
    /// statuses but never sums or averages.</para>
    ///
    /// Has <b>no EF Core / HttpContext dependency</b>, mirroring <see cref="MeetingHistoryService"/>
    /// and <see cref="KpiScopeService"/>, so it is fully unit-testable. See ADR 0001/0002 and CONTEXT.md.
    /// </summary>
    public static class KpiHistoryService
    {
        /// <summary>
        /// The Period a Week belongs to, keyed by its Monday <paramref name="weekStart"/>.
        /// A Week whose Monday is in (say) June belongs wholly to June even if its later days
        /// run into July.
        /// </summary>
        public static KpiPeriod PeriodOf(DateOnly weekStart, PeriodGranularity granularity) => granularity switch
        {
            PeriodGranularity.Month => new(granularity, weekStart.Year, weekStart.Month),
            PeriodGranularity.Quarter => new(granularity, weekStart.Year, (weekStart.Month - 1) / 3 + 1),
            PeriodGranularity.Year => new(granularity, weekStart.Year, 0),
            _ => throw new ArgumentOutOfRangeException(nameof(granularity), granularity, null)
        };

        /// <summary>
        /// Groups a single KPI's weekly <paramref name="results"/> into Periods at the given
        /// <paramref name="granularity"/>, with the Periods (and the Weeks within each) in
        /// chronological sequence. Each group carries its results verbatim and a status tally
        /// — no value is ever summed or averaged.
        /// </summary>
        public static IReadOnlyList<KpiPeriodGroup> GroupByPeriod(
            IEnumerable<KpiResult> results, PeriodGranularity granularity) =>
            results
                .GroupBy(r => PeriodOf(r.WeekStart, granularity))
                .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Index)
                .Select(g =>
                {
                    var ordered = g.OrderBy(r => r.WeekStart).ToList();
                    return new KpiPeriodGroup
                    {
                        Period = g.Key,
                        Results = ordered,
                        StatusTally = ordered
                            .GroupBy(r => r.Status)
                            .ToDictionary(s => s.Key, s => s.Count())
                    };
                })
                .ToList();
    }
}
