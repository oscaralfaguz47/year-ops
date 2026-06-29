namespace OceansApp.Models.Domain.WeeklyPulse
{
    /// <summary>
    /// How a KPI presents in the Weekly Pulse Review (meeting). The scope gate decides
    /// whether it appears at all; among those that appear, the "Green-quiet" rule decides
    /// how loudly. See <see cref="ReviewSurfacingService.SurfaceKpi"/> and CONTEXT.md.
    /// </summary>
    public enum KpiSurfacing
    {
        /// <summary>Out of meeting scope (or retired): never appears in the Review.</summary>
        Hidden = 0,

        /// <summary>In scope with a Green result: appears but stays quiet.</summary>
        Quiet = 1,

        /// <summary>In scope with a Red/Yellow/missing result: demands attention.</summary>
        Loud = 2
    }
}
