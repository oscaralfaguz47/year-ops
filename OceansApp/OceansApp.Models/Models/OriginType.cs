namespace OceansApp.Models.Models
{
    /// <summary>
    /// The kind of record a Living entity (<see cref="Issue"/>, <see cref="ToDo"/>) was
    /// converted <i>from</i>. Paired with an origin id it forms the conversion
    /// back-reference: a Conversion is always <b>additive</b> — the source is preserved
    /// and the new entity simply remembers where it came from. See the Conversion entry
    /// in docs/oce-weekly-pulse/CONTEXT.md.
    /// </summary>
    public enum OriginType
    {
        /// <summary>
        /// Converted from a check-in (segue) into an Issue. Retained for historical Issues
        /// raised before the check-in was removed as an entity (ADR 0003); no new conversion
        /// produces this origin.
        /// </summary>
        CheckIn = 0,

        /// <summary>Converted from a <see cref="Headline"/> into an Issue.</summary>
        Headline = 1,

        /// <summary>Converted from an <see cref="Issue"/> into a To-Do.</summary>
        Issue = 2
    }
}
