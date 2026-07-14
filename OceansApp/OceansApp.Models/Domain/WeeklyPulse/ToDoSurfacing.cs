namespace OceansApp.Models.Domain.WeeklyPulse
{
    /// <summary>
    /// How a To-Do presents in the Weekly Pulse Review (meeting). Every non-Done To-Do
    /// surfaces; only the emphasis differs. A Done To-Do is dropped. Mirrors the shape of
    /// <see cref="KpiSurfacing"/>. See <see cref="ReviewSurfacingService.SurfaceToDo"/> and
    /// CONTEXT.md.
    /// </summary>
    public enum ToDoSurfacing
    {
        /// <summary>Done: the To-Do is finished and never appears in the Review.</summary>
        Hidden = 0,

        /// <summary>Open: surfaces but stays quiet.</summary>
        Quiet = 1,

        /// <summary>Blocked: surfaces loud, demanding attention.</summary>
        Loud = 2
    }
}
