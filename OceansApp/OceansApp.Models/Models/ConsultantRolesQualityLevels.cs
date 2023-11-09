using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OceansApp.Models.Models
{
    public class ConsultantRolesQualityLevels
    {
        public int ConsultantRoleId { get; set; }
        [ForeignKey("ConsultantRoleId")]
        [ValidateNever]
        public ConsultantRole ConsultantRole { get; set; }
        public int ConsultantQualityLevelId { get; set; }
        [ForeignKey("ConsultantQualityLevelId")]
        [ValidateNever]
        public ConsultantQualityLevel ConsultantQualityLevel { get; set; }
        [Required]
        public decimal ConsultantMaximumAmount { get; set; }
        [Required]
        public decimal ClientRateMaximumAmount { get; set; }
        [Required]
        public DateTime UpdatedDate { get; set; }
        [MaxLength(450)]
        public string? CreatedBy { get; set; }

        [ForeignKey("CreatedBy")]
        [ValidateNever]
        public string? UpdatedBy { get; set; }

        [ForeignKey("UpdatedBy")]
        [ValidateNever]
        public ApplicationUser ApplicationUser { get; set; }
        public int? ConsultantSeniorityId { get; set; } 
        [ForeignKey("ConsultantSeniorityId")]
        [ValidateNever]
        public ConsultantSeniority ConsultantSeniority { get; set; }
    }
}
