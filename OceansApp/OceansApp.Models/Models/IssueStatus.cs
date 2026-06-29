namespace OceansApp.Models.Models
{
    /// <summary>
    /// The lifecycle state of a Weekly Pulse <see cref="Issue"/> (a Living entity).
    /// Moves Open -> Deferred -> Solved; Solved is terminal. State is never stored on
    /// the Issue itself — it is derived from the latest status row in the
    /// status/comment history (see glossary in docs/oce-weekly-pulse/CONTEXT.md).
    /// </summary>
    public enum IssueStatus
    {
        Open = 0,
        Deferred = 1,
        Solved = 2
    }
}
