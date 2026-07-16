using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OceansApp.Models.Models
{
    /// <summary>
    /// A Weekly Pulse KPI result — a <b>Snapshot entity</b>: one Team's actual result for one
    /// <see cref="KpiDefinition"/> in one Week. There is exactly one per (KPI, Week): the row
    /// carries a <see cref="WeekStart"/> column (no Week table, see ADR 0001) and re-saving for
    /// the same (KPI, Week) overwrites the existing row rather than duplicating it. A new Week
    /// starts blank.
    ///
    /// The <see cref="Value"/> is <b>free text</b> (e.g. "92%", "3 days") and the
    /// <see cref="Status"/> is a manually chosen Green/Yellow/Red judgement — there is no
    /// arithmetic comparison to the target. Unlike the guarded KPI definition, recording a
    /// result is a frictionless everyday edit. See glossary in docs/oce-weekly-pulse/CONTEXT.md.
    /// </summary>
    public class KpiResult
    {
        [Key]
        public int KpiResultId { get; set; }

        [Required]
        public int KpiDefinitionId { get; set; }

        [ForeignKey("KpiDefinitionId")]
        [ValidateNever]
        public KpiDefinition KpiDefinition { get; set; }

        /// <summary>The Monday that begins the Costa Rica week this snapshot belongs to.</summary>
        [Required]
        public DateOnly WeekStart { get; set; }

        /// <summary>Free-text result for the Week, e.g. "92%", "3 days", "$48k".</summary>
        [Required]
        [MaxLength(200)]
        public string Value { get; set; }

        /// <summary>Manually chosen status — a judgement, not computed from the target.</summary>
        [Required]
        public KpiStatus Status { get; set; }

        /// <summary>
        /// Whether this Week's result surfaces in the Weekly Pulse Review — a per-Week decision
        /// recorded on the result (via the 'Include in Weekly Pulse Review' checkbox), defaulting
        /// to true. Un-ticking it drops this KPI from the Week's Review without affecting Readiness
        /// (which counts every Active KPI's result regardless of inclusion). See KpiScopeService.
        /// </summary>
        [Required]
        public bool IncludeInReview { get; set; } = true;

        [MaxLength(2000)]
        public string? Notes { get; set; }
    }
}
