
using System.ComponentModel.DataAnnotations;

namespace OceansApp.Models.Models
{
    public class ProviderCategory
    {
        [Key]
        public int Id { get; set; }
        [MaxLength(8)]
        [Required]
        public string ProviderCategoryCode { get; set; }
        [Required]
        [MaxLength(40)]
        public string Description { get; set; }
        [Required]
        public DateTime CreateDate { get; set; }
        [MaxLength(8)]
        public string CompanyId { get; set; }

    }
}
