
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OceansApp.Models.Models
{
    public class PaymentSheet
    {
        [Key]
        [Required]
        public int PaymentSheetId { get; set; }
        [Required]
        [Column(TypeName = "date")]
        public DateTime DateFrom { get; set; }
        [Required]
        [Column(TypeName = "date")]
        public DateTime DateTo { get; set; }
        [Required]
        public int PaymentPeriod { get; set; }
        [Required]
        public DateTime CreationDate { get; set; }
        [Required]
        public int ConsultantIdCreatedBy { get; set; }

        [ValidateNever]
        public ConsultantDetail ConsultantDetailCreatedBy { get; set; }
    }
}
