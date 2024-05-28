
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace OceansApp.Models.Models
{
    public class Partner
    {
        [Key]
        [Required]
        public int PartnerId { get; set; }
        [MaxLength(150)]
        [Required]
        public string Name { get; set; }
        [MaxLength(30)]
        public string? Contact { get; set; }
        [MaxLength(30)]
        public string? ContactOccupation { get; set; }
        [MaxLength(150)]
        [Required]
        public string ContactEmail { get; set; }
        [MaxLength(50)]
        public string? Phone { get; set; }
        [Required]
        public DateTime AdmissionDate { get; set; }

        [Required]
        public bool IsActive { get; set; }
        public DateTime? DateLastUpdate { get; set; }
        [MaxLength(160)]
        public string? Address { get; set; }
        [Required]
        public DateTime CreationDate { get; set; }
        [MaxLength(8)]
        public string CompanyId { get; set; }
        public string? AdditionalEmailsForNotifications { get; set; }
        [Required]
        [MaxLength(4)]
        public string IdCountry { get; set; }

        [ValidateNever]
        public Country Country { get; set; }
    }
}
