using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OceansApp.Models.Models
{
    public class TimeOffRequest
    {
        [Key]
        [Required]
        public int TimeOffRequestId { get; set; }

        [Required]
        public int ConsultantId { get; set; }

        [Required]
        [MaxLength(10)]
        public string TimeOffType { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required]
        public int BusinessDays { get; set; }

        [Required]
        public int TransactionStatusId { get; set; }

        [Required]
        public DateTime CreationDate { get; set; }

        [Required]
        [MaxLength(450)]
        public string UserCreatedBy { get; set; }

        [MaxLength(450)]
        public string? UserActionedBy { get; set; }

        public DateTime? ActionDate { get; set; }

        [MaxLength(500)]
        public string? RejectionComment { get; set; }

        [ValidateNever]
        public ConsultantDetail ConsultantDetail { get; set; }

        [ValidateNever]
        public TransactionStatus TransactionStatus { get; set; }

        [ValidateNever]
        public ApplicationUser ApplicationUserCreated { get; set; }

        [ValidateNever]
        public ApplicationUser? ApplicationUserActioned { get; set; }
    }
}
