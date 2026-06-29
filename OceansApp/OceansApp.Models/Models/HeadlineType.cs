namespace OceansApp.Models.Models
{
    /// <summary>
    /// The single type a Weekly Pulse <see cref="Headline"/> carries — the meeting's
    /// good-news/bad-news round. See glossary in docs/oce-weekly-pulse/CONTEXT.md.
    /// </summary>
    public enum HeadlineType
    {
        /// <summary>A win to celebrate — kept quiet in the Review news round.</summary>
        Highlight = 0,

        /// <summary>A concern to flag — surfaced loud in the Review news round.</summary>
        Risk = 1
    }
}
