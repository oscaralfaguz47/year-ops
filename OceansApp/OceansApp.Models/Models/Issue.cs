using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OceansApp.Models.Models
{
    /// <summary>
    /// A Weekly Pulse Issue — the first <b>Living entity</b> (ADR 0001). Created once
    /// and carries a single identity across every Week it touches: it is never copied
    /// forward. It is stamped with an <see cref="OriginWeekStart"/> (the Week it was
    /// raised in) and a <see cref="Priority"/> that is a <i>label only</i> — priority
    /// never gates Review surfacing.
    ///
    /// The Issue itself stores no current status: its state moves Open -> Deferred ->
    /// Solved and is derived from the latest status row in <see cref="History"/> (see
    /// <see cref="IssueHistory"/>). See glossary in docs/oce-weekly-pulse/CONTEXT.md.
    /// </summary>
    public class Issue
    {
        [Key]
        public int IssueId { get; set; }

        [Required]
        public int TeamId { get; set; }

        [ForeignKey("TeamId")]
        [ValidateNever]
        public Team Team { get; set; }

        [Required]
        [MaxLength(2000)]
        public string Title { get; set; }

        [Required]
        public IssuePriority Priority { get; set; }

        /// <summary>The Monday (Costa Rica time) of the Week this Issue was raised in.</summary>
        [Required]
        public DateOnly OriginWeekStart { get; set; }

        /// <summary>The week-stamped status/comment history — one row per change.</summary>
        [ValidateNever]
        public ICollection<IssueHistory> History { get; set; } = new List<IssueHistory>();
    }
}
