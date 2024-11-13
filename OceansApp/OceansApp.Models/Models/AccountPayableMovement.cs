
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace OceansApp.Models.Models
{
    public class AccountPayableMovement
    {
        [Required]
        [Key]
        public int Id { get; set; }
        public int? MovementId { get; set; }
        public int? ProjectId { get; set; }
        [Required]
        [MaxLength(100)]
        public string Description { get; set; }
        [Required]
        [MaxLength(20)]
        public string Type { get; set; }
        public int? MovementTypeId { get; set; }
        [Required]
        public decimal Quantity { get; set; }
        [Required]
        public decimal UnitPrice { get; set; }
        [Required]
        public int AccountPayableId { get; set; }



        [ValidateNever]
        public Project? Project { get; set; }
        [ValidateNever]
        public ReportingMyTimeMovementType MovementType { get; set; }
        [ValidateNever]
        public AccountPayable AccountPayable { get; set; }
    }
}
