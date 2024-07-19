
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace OceansApp.Models.Models
{
    public class ProjectConsultantAssignedHistory
    {
        [Key]
        [Required]
        public int Id { get; set; }
        [Required]
        public int ProjectConsultantAssignedId { get; set; }
        [Required]
        public bool IsActive { get; set; }
        public decimal? HourlyClientRate { get; set; }
        public decimal? HourlySalary { get; set; }
        public decimal? MonthlyClientRate { get; set; }
        public decimal? MonthlySalary { get; set; }
        public decimal? MonthlySalaryPartner { get; set; }
        public bool? IsMonthlySalaryCalculatedPerHour { get; set; }
        [Required]
        public bool AccessToTrackingTool { get; set; }
        [Required]
        public bool IsDefaultProject { get; set; }
        [Required]
        public int PositionId { get; set; }
        public int? PartnerId { get; set; }
        public bool? PartnerPaysBenefits { get; set; }
        [Required]
        public bool HolidaysMustBePaid { get; set; }
        [Required]
        public DateTime ActionDate { get; set; }
        [Required]
        public DateTime CreationDate { get; set; }
        [Required]
        public string UserIdActionedBy { get; set; }


        [ValidateNever]
        public Partner? Partner { get; set; }
        [ValidateNever]
        public ConsultantPosition ConsultantPosition { get; set; }

        [ValidateNever]
        public ProjectConsultantAssigned ProjectConsultantAssigned { get; set; }
        [ValidateNever]
        public ApplicationUser UserActionedBy { get; set; }

    }
}
