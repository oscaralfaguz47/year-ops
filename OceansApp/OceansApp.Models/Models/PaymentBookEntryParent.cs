
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace OceansApp.Models.Models
{
    public class PaymentBookEntryParent
    {
        [Key]
        [Required]
        public int ParentId { get; set; }
        [Required]
        public int TransactionStatusId { get; set; }
        [Required]
        [MaxLength(8)]
        public string CompanyId { get; set; }
        [Required]
        public DateTime CreationDate { get; set; }
        [Required]
        [MaxLength(450)]
        public string UserCreatedBy { get; set; }
        [Required]
        public int DownloadsNumber { get; set; }


        [ValidateNever]
        public ApplicationUser ApplicationUserCreatedBy { get; set; }
        [ValidateNever]
        public TransactionStatus TransactionStatus { get; set; }
        [ValidateNever]
        public Company Company { get; set; }
    }
}
