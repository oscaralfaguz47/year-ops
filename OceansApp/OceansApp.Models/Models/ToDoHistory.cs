using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OceansApp.Models.Models
{
    /// <summary>
    /// One row in a <see cref="ToDo"/>'s week-stamped status/comment history —
    /// <b>one row per change</b>, appended and never updated. Mirrors
    /// <see cref="IssueHistory"/>: a <see cref="ToDoChangeType.Status"/> row records a
    /// transition (its <see cref="Status"/>), a <see cref="ToDoChangeType.Comment"/> row
    /// records a comment (its <see cref="Comment"/>). The To-Do's state for a given Week is
    /// the latest Status row with WeekStart on or before that Week.
    /// </summary>
    public class ToDoHistory
    {
        [Key]
        public int ToDoHistoryId { get; set; }

        [Required]
        public int ToDoId { get; set; }

        [ForeignKey("ToDoId")]
        [ValidateNever]
        public ToDo ToDo { get; set; }

        /// <summary>The Monday (Costa Rica time) of the Week this change was recorded in.</summary>
        [Required]
        public DateOnly WeekStart { get; set; }

        [Required]
        public ToDoChangeType ChangeType { get; set; }

        /// <summary>The new status — set only when <see cref="ChangeType"/> is Status.</summary>
        public ToDoStatus? Status { get; set; }

        /// <summary>The comment text — set only when <see cref="ChangeType"/> is Comment.</summary>
        [MaxLength(2000)]
        public string? Comment { get; set; }

        /// <summary>When the change was recorded; orders changes within the same Week.</summary>
        [Required]
        public DateTimeOffset CreatedAt { get; set; }
    }
}
