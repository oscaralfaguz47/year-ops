using System.ComponentModel.DataAnnotations;

namespace OceansApp.Models.Models
{
    public class AdminPtoConfiguration
    {
        [Key]
        public int AdminPtoConfigurationId { get; set; }

        [Required]
        public decimal AnnualPaidDays { get; set; }
    }
}
