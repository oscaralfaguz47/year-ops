namespace OceansApp.Models.Models
{
    /// <summary>
    /// The manually chosen status of a Weekly Pulse <see cref="KpiResult"/>. It is a
    /// <b>judgement</b>, not an arithmetic comparison to the target — the person recording
    /// the weekly result picks the colour. See glossary in docs/oce-weekly-pulse/CONTEXT.md.
    /// </summary>
    public enum KpiStatus
    {
        Green = 0,
        Yellow = 1,
        Red = 2
    }
}
