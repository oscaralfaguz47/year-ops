namespace OceansApp.Models.Domain.WeeklyPulse
{
    /// <summary>
    /// The granularity by which KPI History groups a single KPI's weekly results into
    /// Periods. A Week belongs <b>wholly</b> to the Period containing its Monday — it is
    /// never split across a boundary. See <see cref="KpiHistoryService"/> and CONTEXT.md.
    /// </summary>
    public enum PeriodGranularity
    {
        Month = 0,
        Quarter = 1,
        Year = 2
    }
}
