using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OceansApp.Models.Models
{
    /// <summary>
    /// One row in an <see cref="Issue"/>'s week-stamped status/comment history —
    /// <b>one row per change</b>, appended and never updated. This is a plain history
    /// table, <i>not</i> an event store: a <see cref="IssueChangeType.Status"/> row
    /// records a transition (its <see cref="Status"/>), a
    /// <see cref="IssueChangeType.Comment"/> row records an IDS comment (its
    /// <see cref="Comment"/>). The Issue's state for a given Week is the latest Status
    /// row with WeekStart on or before that Week.
    /// </summary>
    public class IssueHistory
    {
        [Key]
        public int IssueHistoryId { get; set; }

        [Required]
        public int IssueId { get; set; }

        [ForeignKey("IssueId")]
        [ValidateNever]
        public Issue Issue { get; set; }

        /// <summary>The Monday (Costa Rica time) of the Week this change was recorded in.</summary>
        [Required]
        public DateOnly WeekStart { get; set; }

        [Required]
        public IssueChangeType ChangeType { get; set; }

        /// <summary>The new status — set only when <see cref="ChangeType"/> is Status.</summary>
        public IssueStatus? Status { get; set; }

        /// <summary>The IDS comment text — set only when <see cref="ChangeType"/> is Comment.</summary>
        [MaxLength(2000)]
        public string? Comment { get; set; }

        /// <summary>When the change was recorded; orders changes within the same Week.</summary>
        [Required]
        public DateTimeOffset CreatedAt { get; set; }
    }
}
