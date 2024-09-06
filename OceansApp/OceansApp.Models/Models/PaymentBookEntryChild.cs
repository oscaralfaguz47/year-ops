
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace OceansApp.Models.Models
{
    public class PaymentBookEntryChild
    {
        [Key]
        [Required]
        public int ChildId { get; set; }
        [Required]
        public int ParentId { get; set; }
        [Required]
        public int ConsultantPaymentId { get; set; }
        [MaxLength(300)]
        public string Notes { get; set; }
        [Required]
        public bool Voided { get; set; }


        [ValidateNever]
        public PaymentBookEntryParent PaymentBookEntryParent { get; set; }
        [ValidateNever]
        public ConsultantPayment ConsultantPayment { get; set; }
    }
}
