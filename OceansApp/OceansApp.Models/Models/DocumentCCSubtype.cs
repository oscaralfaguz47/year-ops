using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;


namespace OceansApp.Models.Models
{
    public class DocumentCCSubtype
    {
        [Required]
        [Key]
        public int DocumentCCSybtypeId { get; set; }
        [Required]
        [MaxLength(3)]
        public string DocumentTypeId { get; set; }
        [Required]
        [MaxLength(25)]
        public string Description { get; set; }
        [Required]
        public int CostCenterId { get; set; }
        [Required]
        public int AccountingAccountId { get; set; }
        [Required]
        [MaxLength(8)]
        public string CompanyId { get; set; }


        [ValidateNever]
        public AccountingAccount AccountingAccount { get; set; }
        [ValidateNever]
        public CostCenter CostCenter { get; set; }
        [ValidateNever]
        public Company Company { get; set; }
        [ValidateNever]
        public DocumentType DocumentType { get; set; }


    }
}
