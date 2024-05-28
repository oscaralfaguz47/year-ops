
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace OceansApp.Models.Models
{
    public class Interview
    {
        [Key]
        [Required]
        public int InterviewId { get; set; }
        [Required]
        public int ConsultantId { get; set; }
        [Required]
        public decimal DurationMinutes { get; set; }
        [Required]
        public DateTime Date { get; set; }
        [Required]
        public DateTime CreationDate { get; set; }
        [Required]
        public int ConsultantIdCreatedBy { get; set; }
        public DateTime? LastUpdateDate { get; set; }
        public int? ConsultantIdLastUpdatedBy { get; set; }
        [Required]
        public int TransactionStatusId { get; set; }

        [ValidateNever]
        public ConsultantDetail ConsultantDetail { get; set; }
        [ValidateNever]
        public ConsultantDetail ConsultantDetailCreatedBy { get; set; }
        [ValidateNever]
        public ConsultantDetail? ConsultantDetailUpdatedBy { get; set; }
        [ValidateNever]
        public TransactionStatus TransactionStatus { get; set; }
    }
}
