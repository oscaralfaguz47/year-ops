namespace OceansApp.Models.Domain.WeeklyPulse
{
    /// <summary>
    /// How a Headline presents in the Weekly Pulse Review news round. Unlike a KPI there
    /// is no Hidden state — <i>every</i> headline surfaces; only the emphasis differs.
    /// See <see cref="ReviewSurfacingService.SurfaceHeadline"/> and CONTEXT.md.
    /// </summary>
    public enum HeadlineEmphasis
    {
        /// <summary>A Highlight (a win): surfaces but stays quiet.</summary>
        Quiet = 0,

        /// <summary>A Risk (a concern): surfaces loud, demanding attention.</summary>
        Loud = 1
    }
}
