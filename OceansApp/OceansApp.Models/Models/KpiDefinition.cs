using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OceansApp.Models.Models
{
    /// <summary>
    /// A Weekly Pulse KPI definition — a <b>structural record</b> owned by a <see cref="Team"/>.
    /// It names a metric, the <see cref="ApplicationUser"/> who owns it, and a free-text weekly
    /// <see cref="Target"/> (e.g. "&gt;= 95%"). Creating, editing and retiring a definition is a
    /// <i>guarded</i> mutation (an explicit confirm precedes the POST) because it reshapes what the
    /// meeting expects — distinct from the frictionless everyday recording of results.
    ///
    /// A single flag governs its lifecycle: <see cref="Active"/> — live vs retired. Retiring keeps
    /// the definition (and any historical results that reference it) but stops it expecting new
    /// input; an Active KPI counts toward Readiness. Whether a KPI appears in the meeting (Review)
    /// is no longer a structural property of the definition — it is a per-Week decision recorded on
    /// each <see cref="KpiResult"/> (see <c>KpiResult.IncludeInReview</c> and <c>KpiScopeService</c>).
    /// See ADR 0002.
    /// </summary>
    public class KpiDefinition
    {
        [Key]
        public int KpiDefinitionId { get; set; }

        [Required]
        public int TeamId { get; set; }

        [ForeignKey("TeamId")]
        [ValidateNever]
        public Team Team { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; }

        [Required]
        public string OwnerId { get; set; }

        [ForeignKey("OwnerId")]
        [ValidateNever]
        public ApplicationUser Owner { get; set; }

        /// <summary>Free-text weekly target, e.g. "&gt;= 95%", "&lt; 3 days", "$50k".</summary>
        [Required]
        [MaxLength(200)]
        public string Target { get; set; }

        /// <summary>Live (true) vs retired (false). Retiring stops it expecting new input.</summary>
        [Required]
        public bool Active { get; set; } = true;
    }
}
