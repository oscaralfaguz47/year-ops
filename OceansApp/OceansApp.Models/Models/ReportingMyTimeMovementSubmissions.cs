

using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace OceansApp.Models.Models
{
    public class ReportingMyTimeMovementSubmissions
    {
        [Key]
        [Required]
        public int SubmissionId { get; set; }
        [Required]
        public DateTime SubmissionDate { get; set; }
        public DateTime? LastSubmissionDate { get; set; }
        [Required]
        public int ProjectId { get; set; }
        [Required]
        public int ConsultantId { get; set; }
        public DateTime StartPeriodDate { get; set; }
        public DateTime EndPeriodDate { get; set; }
        [Required]
        public int TransactionStatusId { get; set; }


        [ValidateNever]
        public Project Project { get; set; }
        [ValidateNever]
        public ConsultantDetail ConsultantDetail { get; set; }
        [ValidateNever]
        public TransactionStatus TransactionStatus { get; set; }
    }
}
