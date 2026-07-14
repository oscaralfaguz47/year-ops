using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OceansApp.Models.Models
{
    /// <summary>
    /// A Weekly Pulse Headline — a <b>Snapshot entity</b> (ADR 0001): a short piece of
    /// weekly news for a Team, of one <see cref="HeadlineType"/> (Highlight — a win, or
    /// Risk — a concern). The row carries a <see cref="WeekStart"/> column (no Week table,
    /// see ADR 0001) and belongs to exactly one Week; a new Week starts blank — headlines
    /// are never copied forward.
    ///
    /// Unlike check-ins and KPI results there may be <b>many</b> headlines per (Team, Week):
    /// in the Review they open the team's segment as a news round where <i>all</i> of the
    /// week's headlines surface, Risk flagged loud and Highlight kept quiet. Headlines are
    /// <b>not pinnable</b> (every one already surfaces, so there is nothing to un-park) —
    /// this entity deliberately carries no pin flag. See glossary in
    /// docs/oce-weekly-pulse/CONTEXT.md.
    /// </summary>
    public class Headline
    {
        [Key]
        public int HeadlineId { get; set; }

        [Required]
        public int TeamId { get; set; }

        [ForeignKey("TeamId")]
        [ValidateNever]
        public Team Team { get; set; }

        /// <summary>The Monday that begins the Costa Rica week this snapshot belongs to.</summary>
        [Required]
        public DateOnly WeekStart { get; set; }

        [Required]
        public HeadlineType Type { get; set; }

        [Required]
        [MaxLength(2000)]
        public string Text { get; set; }
    }
}
