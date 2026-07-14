using OceansApp.Models.Domain.WeeklyPulse;
using OceansApp.Models.Models;

namespace OceansApp.Models.ViewModels.WeeklyPulse
{
    /// <summary>
    /// Backs the KPI History picker: the KPI definitions whose weekly results can be read
    /// back as a Period-grouped history. Selecting one opens its <see cref="KpiHistoryVM"/>.
    /// </summary>
    public class KpiHistoryIndexVM
    {
        public List<KpiDefinition> Kpis { get; set; } = new();
    }

    /// <summary>
    /// Backs the KPI History for a SINGLE KPI: its weekly results read in sequence and
    /// grouped by the selected <see cref="Granularity"/> (month / quarter / year), per
    /// <c>KpiHistoryService.GroupByPeriod</c>. Display only — the per-Period tally counts
    /// statuses but never sums or averages the free-text values.
    /// </summary>
    public class KpiHistoryVM
    {
        public KpiDefinition Kpi { get; set; }
        public PeriodGranularity Granularity { get; set; }
        public IReadOnlyList<KpiPeriodGroup> Periods { get; set; } = new List<KpiPeriodGroup>();
    }
}
