namespace OceansApp.Models.Models
{
    /// <summary>
    /// The lifecycle state of a Weekly Pulse <see cref="ToDo"/> (a Living entity).
    /// Moves Open -> Blocked -> Done; Done is terminal. State is never stored on the
    /// ToDo itself — it is derived from the latest status row in the status/comment
    /// history (mirrors <see cref="IssueStatus"/>; see glossary in
    /// docs/oce-weekly-pulse/CONTEXT.md).
    /// </summary>
    public enum ToDoStatus
    {
        Open = 0,
        Blocked = 1,
        Done = 2
    }
}
