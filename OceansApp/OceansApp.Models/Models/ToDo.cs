using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OceansApp.Models.Models
{
    /// <summary>
    /// A Weekly Pulse To-Do — a <b>Living entity</b> (ADR 0001), like an <see cref="Issue"/>.
    /// Created once and carries a single identity across every Week it touches: it is never
    /// copied forward. It is stamped with an <see cref="OriginWeekStart"/> (the Week it was
    /// raised in), has a single <see cref="Owner"/> (the <see cref="ApplicationUser"/>
    /// accountable for it) and a <see cref="DueDate"/>.
    ///
    /// The To-Do itself stores no current status: its state moves Open -> Blocked -> Done and
    /// is derived from the latest status row in <see cref="History"/> (see
    /// <see cref="ToDoHistory"/>). It surfaces on the Dashboard until Done; in the Review every
    /// non-Done To-Do surfaces, Blocked flagged loud (see <c>ReviewSurfacingService.SurfaceToDo</c>).
    /// See glossary in docs/oce-weekly-pulse/CONTEXT.md.
    /// </summary>
    public class ToDo
    {
        [Key]
        public int ToDoId { get; set; }

        [Required]
        public int TeamId { get; set; }

        [ForeignKey("TeamId")]
        [ValidateNever]
        public Team Team { get; set; }

        [Required]
        [MaxLength(2000)]
        public string Title { get; set; }

        /// <summary>The single user accountable for the To-Do.</summary>
        [Required]
        public string OwnerId { get; set; }

        [ForeignKey("OwnerId")]
        [ValidateNever]
        public ApplicationUser Owner { get; set; }

        /// <summary>When the To-Do is due.</summary>
        [Required]
        public DateOnly DueDate { get; set; }

        /// <summary>The Monday (Costa Rica time) of the Week this To-Do was raised in.</summary>
        [Required]
        public DateOnly OriginWeekStart { get; set; }

        /// <summary>The week-stamped status/comment history — one row per change.</summary>
        [ValidateNever]
        public ICollection<ToDoHistory> History { get; set; } = new List<ToDoHistory>();
    }
}
