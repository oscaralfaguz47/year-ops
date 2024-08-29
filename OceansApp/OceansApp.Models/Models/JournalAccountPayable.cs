
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace OceansApp.Models.Models
{
    public class JournalAccountPayable
    {
        [Required]
        [Key]
        public int JournalId { get; set; }
        [Required]
        [MaxLength(8)]
        public string CompanyId { get; set; }
        [Required]
        public DateTime StartDatePeriod { get; set; }
        [Required]
        public DateTime EndDatePeriod { get; set; }
        [Required]
        public int TransactionStatusId { get; set; }
        [Required]
        [MaxLength(10)]
        public string Entry { get; set; }
        [Required]
        [MaxLength(4)]
        public string AccountingPackage { get; set; }
        [Required]
        [MaxLength(4)]
        public string EntryType { get; set; }
        [Required]
        public DateTime AccountingDate { get; set; }
        [Required]
        public DateTime CreationDate { get; set; }
        [Required]
        [MaxLength(450)]
        public string UserCreatedBy { get; set; }
        public DateTime? LastUpdatedDate { get; set; }
        [MaxLength(450)]
        public string? UserLastUpdatedBy { get; set; }


        [ValidateNever]
        public ApplicationUser ApplicationUserCreatedBy { get; set; }
        [ValidateNever]
        public ApplicationUser? ApplicationUserUpdatedBy { get; set; }
        [ValidateNever]
        public TransactionStatus TransactionStatus { get; set; }
        [ValidateNever]
        public Company Company { get; set; }
    }
}
