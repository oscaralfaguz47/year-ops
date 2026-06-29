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
    }
}
