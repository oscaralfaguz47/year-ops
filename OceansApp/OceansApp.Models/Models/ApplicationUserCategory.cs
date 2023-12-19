
using System.ComponentModel.DataAnnotations;

namespace OceansApp.Models.Models
{
    public class ApplicationUserCategory
    {
        [Key]
        public int UserCategoryId { get; set; }
        [Required]
        [MaxLength(50)]
        public string Name { get; set; }
    }
}
