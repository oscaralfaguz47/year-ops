
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
        public decimal? MonthlySalaryThirdParty { get; set; }
        public bool? IsMonthlySalaryCalculatedPerHour { get; set; }
        public bool? AccessToTrackingTool { get; set; }
        public bool IsDefaultProject { get; set; }
        public int? PositionId { get; set; }


        [ValidateNever]
        public ConsultantDetail ConsultantDetail { get; set; }
        [ValidateNever]
        public Project Project { get; set; }
        [ValidateNever]
        public ConsultantPosition ConsultantPosition { get; set; }
    }
}
