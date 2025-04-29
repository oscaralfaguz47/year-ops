using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace OceansApp.Models.Models
{
    public class DocumentType
    {
        [Required]
        [MaxLength(3)]
        [Key]
        public string DocumentTypeId { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        public int TransactionTypeId { get; set; }

        [ValidateNever]
        public TransactionType TransactionType { get; set; }
    }
}
