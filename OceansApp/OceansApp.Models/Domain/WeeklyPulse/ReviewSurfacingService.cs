using OceansApp.Models.Models;

namespace OceansApp.Models.Domain.WeeklyPulse
{
    /// <summary>
    /// Pure domain logic for the Weekly Pulse Review (meeting) view's Issue surfacing.
    ///
    /// Ported from the prototype's <c>model.js</c> <c>reviewSurfacing</c>: in Review,
    /// Open issues auto-surface and a <see cref="IssueStatus.Deferred"/> issue stays
    /// quiet unless it is <b>pinned</b> (the pin un-parks it for the meeting). Solved
    /// issues are done and never surface. The pin is a Deferred-only override — see
    /// <see cref="EnsurePinnable"/>.
    ///
    /// Has <b>no EF Core / HttpContext dependency</b>: it operates on a derived state
    /// plus a pin flag, so it is fully unit-testable. See ADR 0001 and CONTEXT.md.
    /// </summary>
    public static class ReviewSurfacingService
    {
        /// <summary>
        /// Whether an Issue surfaces in Review: iff
        /// <c>(state == Open || pinned) &amp;&amp; state != Solved</c>.
        /// </summary>
        public static bool Surfaces(IssueStatus state, bool pinned) =>
            (state == IssueStatus.Open || pinned) && state != IssueStatus.Solved;

        /// <summary>
        /// Guards the pin override: the pin is offered only on Deferred issues (it
        /// un-parks them for the meeting). Open issues already surface and Solved ones
        /// are done, so pinning does real work only on a Deferred issue — pinning any
        /// other state is rejected at the model level. Throws when
        /// <paramref name="state"/> is not <see cref="IssueStatus.Deferred"/>.
        /// </summary>
        public static void EnsurePinnable(IssueStatus state)
        {
            if (state != IssueStatus.Deferred)
                throw new InvalidOperationException(
                    $"Only Deferred issues are pinnable (state is {state}).");
        }

        /// <summary>
        /// How a KPI surfaces in Review. The scope gate comes first: a KPI that is not in
        /// meeting scope (per <see cref="KpiScopeService.SurfacesInReview"/>) is
        /// <see cref="KpiSurfacing.Hidden"/> regardless of its result. Among in-scope KPIs the
        /// Green-quiet rule applies: a Green result is <see cref="KpiSurfacing.Quiet"/>, while
        /// any other status — or a missing result (<paramref name="result"/> is <c>null</c>) —
        /// is <see cref="KpiSurfacing.Loud"/>.
        /// </summary>
        public static KpiSurfacing SurfaceKpi(KpiDefinition kpi, KpiResult? result)
        {
            if (!KpiScopeService.SurfacesInReview(kpi))
                return KpiSurfacing.Hidden;

            return result is { Status: KpiStatus.Green } ? KpiSurfacing.Quiet : KpiSurfacing.Loud;
        }

        /// <summary>
        /// How a Headline surfaces in the Review news round. Unlike a KPI there is no scope
        /// gate and no Hidden outcome: <i>every</i> headline surfaces (it is the meeting's
        /// good-news/bad-news round). A <see cref="HeadlineType.Risk"/> is flagged
        /// <see cref="HeadlineEmphasis.Loud"/>, while a <see cref="HeadlineType.Highlight"/>
        /// is kept <see cref="HeadlineEmphasis.Quiet"/>. Headlines are not pinnable — there
        /// is nothing to un-park because they all already surface.
        /// </summary>
        public static HeadlineEmphasis SurfaceHeadline(HeadlineType type) =>
            type == HeadlineType.Risk ? HeadlineEmphasis.Loud : HeadlineEmphasis.Quiet;

        /// <summary>
        /// Whether a To-Do shows on the Dashboard: it surfaces until <see cref="ToDoStatus.Done"/>,
        /// i.e. every non-Done To-Do is shown. A Done To-Do drops off.
        /// </summary>
        public static bool ToDoShowsOnDashboard(ToDoStatus state) => state != ToDoStatus.Done;

        /// <summary>
        /// How a To-Do surfaces in Review. Unlike a KPI there is no scope gate: <i>every</i>
        /// non-Done To-Do surfaces. A <see cref="ToDoStatus.Done"/> To-Do is dropped
        /// (<see cref="ToDoSurfacing.Hidden"/>), a <see cref="ToDoStatus.Blocked"/> To-Do is
        /// flagged <see cref="ToDoSurfacing.Loud"/>, and an <see cref="ToDoStatus.Open"/> To-Do
        /// is kept <see cref="ToDoSurfacing.Quiet"/>.
        /// </summary>
        public static ToDoSurfacing SurfaceToDo(ToDoStatus state) => state switch
        {
            ToDoStatus.Done => ToDoSurfacing.Hidden,
            ToDoStatus.Blocked => ToDoSurfacing.Loud,
            _ => ToDoSurfacing.Quiet
        };
    }
}
