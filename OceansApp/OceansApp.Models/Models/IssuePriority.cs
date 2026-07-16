namespace OceansApp.Models.Models
{
    /// <summary>
    /// The priority of a Weekly Pulse <see cref="Issue"/>. This is a <b>label only</b>:
    /// it never gates Review surfacing (surfacing is state-only). See glossary in
    /// docs/oce-weekly-pulse/CONTEXT.md.
    /// </summary>
    public enum IssuePriority
    {
        Low = 0,
        Med = 1,
        High = 2,
        Critical = 3
    }
}
