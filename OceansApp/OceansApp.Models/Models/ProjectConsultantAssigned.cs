
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace OceansApp.Models.Models
{
    public class ProjectConsultantAssigned
    {
        [Key]
        [Required]
        public int ProjectConsultantAssignedId { get; set; }
        [Required]
        public int ProjectId { get; set; }
        [Required]
        public int ConsultantId { get; set; }
        [Required]
        public DateTime CreationDate { get; set; }
        [Required]
        public bool IsActive { get; set; }
        public decimal? HourlyClientRate { get; set; }
        public decimal? HourlySalary { get; set; }
        public decimal? MonthlyClientRate { get; set; }
        public decimal? MonthlySalary { get; set; }
        public bool? IsMonthlySalaryCalculatedPerHour { get; set; }
        [Required]
        [MaxLength(130)]
        public string PositionDetail { get; set; }

        [ValidateNever]
        public ConsultantDetail ConsultantDetail { get; set; }
        [ValidateNever]
        public Project Project { get; set; }
    }
}
