
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OceansApp.Models.Models
{
    public class Provider
    {
        [Key]
        public int ProviderId { get; set; }
        [MaxLength(20)]
        [Required]
        public string ProviderCode { get; set; }
        [MaxLength(150)]
        [Required]
        public string Name { get; set; }
        [MaxLength(150)]
        public string? Alias { get; set; }
        [MaxLength(30)] 
        [Required]
        public string Occupation { get; set; }
        public string? Address { get; set; }
        [MaxLength(249)]
        public string? Email { get; set; }
        [Required]
        public DateTime AdmissionDate { get; set; }
        [MaxLength(50)]
        public string? Phone1 { get; set; }
        [MaxLength(50)]
        public string? Phone2 { get; set; }
        [MaxLength(4)]
        [Required]
        public string IdCountry { get; set; }
        [ForeignKey("IdCountry")]
        [ValidateNever]
        public Country Country { get; set; }
        [MaxLength(8)]
        [Required]
        public int Id { get; set; }
        [ForeignKey("Id")]
        [ValidateNever]
        public ProviderCategory ProviderCategory { get; set; }
        public string? Notes { get; set; }
        [MaxLength(1)]
        [Required]
        public string IsActive { get; set; }
        public DateTime? DateLastUpdate { get; set; }
        [Required]
        public DateTime CreationDate { get; set; }

        [MaxLength(8)]
        public string CompanyId { get; set; }

        public int? ClientId { get; set; }
        [ValidateNever]
        public Client Client  { get; set; }
        [MaxLength(249)]
        public string? PersonalEmail { get; set; }
        [MaxLength(3)]
        public string? ConsultantCategory { get; set; }
        public decimal? HourlySalary { get; set; }
        public decimal? MonthlySalary { get; set; }
        public decimal? HourlyClientRate { get; set; }
        public decimal? MonthlyClientRate { get; set; }
        public bool? IsTheMonthlyClientRateCalculatePerHour { get; set; }
        public string? Location { get; set; }
        [MaxLength(20)]
        public string? ShirtSize { get; set; }
    }
}
