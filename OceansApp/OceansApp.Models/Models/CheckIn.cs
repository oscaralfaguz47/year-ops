using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OceansApp.Models.Models
{
    /// <summary>
    /// A Weekly Pulse check-in (segue) — a <b>Snapshot entity</b> owned by the Team
    /// Leader. There is exactly one per (Team, Week): the row carries a
    /// <see cref="WeekStart"/> column (no Week table, see ADR 0001) and re-saving for
    /// the same (Team, Week) overwrites the existing row rather than duplicating it.
    /// A new Week starts blank. See glossary in docs/oce-weekly-pulse/CONTEXT.md.
    /// </summary>
    public class CheckIn
    {
        [Key]
        public int CheckInId { get; set; }

        [Required]
        public int TeamId { get; set; }

        [ForeignKey("TeamId")]
        [ValidateNever]
        public Team Team { get; set; }

        /// <summary>The Monday that begins the Costa Rica week this snapshot belongs to.</summary>
        [Required]
        public DateOnly WeekStart { get; set; }

        [Required]
        public CheckInType Type { get; set; }

        [MaxLength(2000)]
        public string Note { get; set; }
    }
}
