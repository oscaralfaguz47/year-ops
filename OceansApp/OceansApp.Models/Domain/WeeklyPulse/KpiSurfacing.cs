namespace OceansApp.Models.Domain.WeeklyPulse
{
    /// <summary>
    /// How this Week's KPI result presents in the Weekly Pulse Review (meeting). The inclusion
    /// gate decides whether it appears at all; among those that appear, the "Green-quiet" rule
    /// decides how loudly. See <see cref="ReviewSurfacingService.SurfaceKpi"/> and CONTEXT.md.
    /// </summary>
    public enum KpiSurfacing
    {
        /// <summary>Not included this Week (or no result recorded): never appears in the Review.</summary>
        Hidden = 0,

        /// <summary>Included with a Green result: appears but stays quiet.</summary>
        Quiet = 1,

        /// <summary>Included with a Red/Yellow result: demands attention.</summary>
        Loud = 2
    }
}
