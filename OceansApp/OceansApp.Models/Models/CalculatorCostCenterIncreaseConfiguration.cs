using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OceansApp.Models.Models
{
    public class CalculatorCostCenterIncreaseConfiguration
    {

        [Required]
        [Key]
        public int CostCenterIncreaseId { get; set; }
        [MaxLength(25)]
        public int CostCenterId { get; set; }
        [ForeignKey("CostCenterId")]
        [ValidateNever]
        public CostCenter CostCenter { get; set; }
        public Double? Increase { get; set; }

        [Required]
        [MaxLength(450)]
        public string? IdUserUpdatedBy { get; set; }

        [ForeignKey("IdUserUpdatedBy")]
        [ValidateNever]
        public ApplicationUser? ApplicationUser { get; set; }
        [Required]
        public DateTime? DateLastUpdate { get; set; }

    }
}
