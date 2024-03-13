
using System.ComponentModel.DataAnnotations;

namespace OceansApp.Models.Models
{
    public class ConsultantBenefit
    {
        [Required]
        [Key]
        public int BenefitId { get; set; }
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }
        public string? Details { get; set; }
        [Required]
        public decimal Amount { get; set; }
        [MaxLength(20)]
        public string BenefitPeriod { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

       
    }
}
