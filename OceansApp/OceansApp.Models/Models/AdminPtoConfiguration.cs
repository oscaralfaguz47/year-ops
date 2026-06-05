using System.ComponentModel.DataAnnotations;

namespace OceansApp.Models.Models
{
    public class AdminPtoConfiguration
    {
        [Key]
        public int AdminPtoConfigurationId { get; set; }

        [Required]
        public decimal AnnualPaidDays { get; set; }

        // Go-live date: admin PTO accrues and usage is counted only from this date forward.
        // Days earned before this date are entered per-consultant via InitialAdminPtoBalance.
        [Required]
        public DateTime EffectiveDate { get; set; }
    }
}
