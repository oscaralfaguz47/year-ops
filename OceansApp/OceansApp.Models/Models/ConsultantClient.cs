
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OceansApp.Models.Models
{
    public class ConsultantClient
    {
        [Required]
        [MaxLength(450)]
        [ForeignKey("Id")]
        public string ConsultantId { get; set; }
        [Required]
        [ForeignKey("ClientId")]
        public int ClientId { get; set; }
        [MaxLength(130)]
        [Required]
        public string PositionDetail { get; set; }
        [Required]
        [ForeignKey("Id")]
        public string SuccessManager { get; set; }
        public double? HourlyClientRate { get; set; }
        public double? HourlySalary { get; set; }
        public double? MonthlyClientRate { get; set; }
        public double? MontlySalary { get; set; }
        public bool? IsTheMonthlyClientRateCalculatePerHour { get; set; }
        [Required]
        public bool IsActive { get; set; }
        [Required]
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        [Required]
        public DateTime CreationDate { get; set; }
        [Required]
        [MaxLength(450)]
        [ForeignKey("Id")]
        public string CreatedBy { get; set; }
        public DateTime? DateLastUpdate { get; set; }
        [MaxLength(450)]
        [ForeignKey("Id")]
        public string? UpdatedBy { get; set; }

        [ValidateNever]
        public Client Client { get; set; }
        [ValidateNever]
        public ApplicationUser ApplicationUser { get; set; }
        [ValidateNever]
        public ApplicationUser ApplicationUserSuccessManager { get; set; }
        [ValidateNever]
        public ApplicationUser ApplicationUserCreate { get; set; }
        [ValidateNever]
        public ApplicationUser ApplicationUserUpdate { get; set; }
    }
}
