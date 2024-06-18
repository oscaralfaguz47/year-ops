
using System.ComponentModel.DataAnnotations;

namespace OceansApp.Models.Models
{
    public class Company
    {
        [Key]
        [Required]
        [MaxLength(8)]
        public string CompanyId { get; set; }
        [MaxLength(150)]
        [Required]
        public string Name { get; set; }
    }
}
