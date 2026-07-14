using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OceansApp.Models.Models
{
    /// <summary>
    /// A functional department (Sales, Marketing, Operations, Leadership) that owns
    /// the Weekly Pulse data. Modeled generically (not "PulseTeam") so it can later be
    /// promoted into a company-wide org concept. See ADR 0002.
    ///
    /// Leader-only: the team carries its leader and a meeting order. There is no
    /// Person-Team membership table; Person is always an <see cref="ApplicationUser"/>.
    /// </summary>
    public class Team
    {
        [Key]
        public int TeamId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [Required]
        public string TeamLeaderId { get; set; }

        [ForeignKey("TeamLeaderId")]
        [ValidateNever]
        public ApplicationUser TeamLeader { get; set; }

        /// <summary>Order in which the team is taken in the weekly meeting.</summary>
        [Required]
        public int DisplayOrder { get; set; }
    }
}
