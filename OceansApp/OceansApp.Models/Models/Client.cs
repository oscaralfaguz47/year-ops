
using System.ComponentModel.DataAnnotations;

namespace OceansApp.Models.Models
{
    public class Client
    {
        [Key]
        public int ClientId { get; set; }
        [MaxLength(20)]
        [Required]
        public string ClientCode { get; set; }
        [MaxLength(150)]
        [Required]
        public string Name { get; set; }
        [MaxLength(150)]
        public string? Alias { get; set; }
        [MaxLength(30)]
        public string? Contact { get; set; }
        [MaxLength(30)]
        public string? ContactOccupation { get; set; }
        [MaxLength(50)]
        public string? Phone1 { get; set; }
        [MaxLength(50)]
        public string? Phone2 { get; set; }
        [Required]
        public DateTime AdmissionDate { get; set; }
        [MaxLength(4)]
        [Required]
        public string PaymentCondition { get; set; }
        [Required]
        public Decimal Discount { get; set; }
        [MaxLength(1)]
        [Required]
        public string IsActive { get; set; }
        [MaxLength(8)]
        [Required]
        public string ClientCategory { get; set; }
        [MaxLength(1)]
        public string? ClientClass { get; set; }
        [MaxLength(249)]
        public string? Emails { get; set; }
        public string? Notes { get; set; }
        public DateTime? DateLastUpdate { get; set; }
        [MaxLength(160)]
        public string? Address { get; set; }
        [Required]
        public DateTime CreationDate { get; set; }
        [MaxLength(8)]
        public string CompanyId { get; set; }
        public int? SuccessManager { get; set; }
        public string? AdditionalEmailsForNotifications { get; set; }
        [Required]
        public Decimal LatePaymentFee { get; set; }
        [Required]
        public bool AllowSentLatePaymentNotifications { get; set; } = true;
    }
}
