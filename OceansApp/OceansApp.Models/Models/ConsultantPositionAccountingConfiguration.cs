using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace OceansApp.Models.Models
{
    public class ConsultantPositionAccountingConfiguration
    {
        [Key]
        [Required]
        public int Id { get; set; }
        [MaxLength(8)]
        [Required]
        public string CompanyId { get; set; }
        [Required]
        public int CostCenterId { get; set; }
        [Required]
        public int AccountingAccountId { get; set; }
        [Required]
        public int MovementTypeId { get; set; }
        [Required]
        public int PositionId { get; set; }

        [ValidateNever]
        public CostCenter CostCenter { get; set; }
        [ValidateNever]
        public AccountingAccount AccountingAccount { get; set; }
        [ValidateNever]
        public ReportingMyTimeMovementType MovementType { get; set; }
        [ValidateNever]
        public ConsultantPosition ConsultantPosition { get; set; }
    }
}
