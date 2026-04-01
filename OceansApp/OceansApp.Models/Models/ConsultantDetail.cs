using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OceansApp.Models.Models
{
    public class ConsultantDetail
    {
        [Required]
        public int ConsultantId { get; set; }
        [ForeignKey("Id")]
        [Required]
        public string UserId { get; set; }
        [Required]
        public DateTime CreationDate { get; set; }
        [MaxLength(450)]
        public string? UserCreatedBy { get; set; }
        public DateTime? LastUpdatedDate { get; set; }
        [MaxLength(450)]
        public string? UserLastUpdatedBy { get; set; }

        [MaxLength(4)]
        public string? IdCountry { get; set; }
        [MaxLength(50)]
        public string? Phone2 { get; set; }
        public string? Address { get; set; }
        [MaxLength(249)]
        public string? PersonalEmail { get; set; }
        public string? Location { get; set; }
        [MaxLength(20)]
        public string? ShirtSize { get; set; }
        [MaxLength(8)]
        public string? CompanyId { get; set; }
        public int? PaymentMethodId { get; set; }
        public int? PaymentPeriod { get; set; }
        public int? ConsultantHolidayId { get; set; }
        [Required]
        public DateTime StartDate { get; set; }
        [Required]
        public int WorkingModel { get; set; }
        public DateTime? OldConsultantSystemStartDate { get; set; }
        public bool IsEligibleForPaidTimeOff { get; set; }
        public int? AnnualPaidTimeOffDays { get; set; }
        public int? InitialPtoBalance { get; set; }


        [ValidateNever]
        public Country Country { get; set; }

        [ValidateNever]
        public ApplicationUser ApplicationUser { get; set; }

        [ValidateNever]
        public ApplicationUser? ApplicationUserCreated { get; set; }
        [ValidateNever]
        public ApplicationUser? ApplicationUserUpdated { get; set; }
        [ValidateNever]
        public PaymentMethod? PaymentMethod { get; set; }

        [ValidateNever]
        public ConsultantHoliday? ConsultantHoliday { get; set; }
        [ValidateNever]
        public virtual ICollection<ProjectConsultantAssigned> ProjectsConsultantsAssigned { get; set; }
        [ValidateNever]
        public virtual ICollection<ReportingMyTimeMovement> ReportingMyTimeMovements { get; set; }

    }
}
