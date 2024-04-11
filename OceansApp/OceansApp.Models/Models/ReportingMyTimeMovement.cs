
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace OceansApp.Models.Models
{
    public class ReportingMyTimeMovement
    {
        [Key]
        [Required]
        public int MovementId { get; set; }
        [Required]
        public int ProjectId { get; set; }
        [Required]
        public int ConsultantId { get; set; }
        [Required]
        public DateTime ActionDate { get; set; }
        public  TimeSpan? TimeFrom { get; set; }
        public TimeSpan? TimeTo { get; set; }
        [Required]
        public decimal Quantity { get; set; }
        [MaxLength(200)]
        public string? Notes { get; set; }
        [Required]
        public DateTime CreationDate { get; set; }
        public DateTime? LastUpdateDate { get; set; }
        public string? AttachmentUrl { get; set; }
        [Required]
        public int TransactionStatusId { get; set; }
        [Required]
        public int MovementTypeId { get; set; }



        [ValidateNever]
        public Project Project { get; set; }
        [ValidateNever]
        public ConsultantDetail ConsultantDetail { get; set; }
        [ValidateNever]
        public TransactionStatus TransactionStatus { get; set; }
        [ValidateNever]
        public ReportingMyTimeMovementType ReportingMyTimeMovementType { get; set; }
    }
}
