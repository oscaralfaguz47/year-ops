
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
        [MaxLength(5)]
        public  string? TimeFrom { get; set; }
        [MaxLength(5)]
        public string? TimeTo { get; set; }
        [Required]
        public decimal Quantity { get; set; }
        [MaxLength(800)]
        public string? Notes { get; set; }
        [Required]
        public DateTime CreationDate { get; set; }
        public DateTime? LastUpdateDate { get; set; }
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
