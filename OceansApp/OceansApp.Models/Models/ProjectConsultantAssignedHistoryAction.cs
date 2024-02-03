
using System.ComponentModel.DataAnnotations;

namespace OceansApp.Models.Models
{
    public class ProjectConsultantAssignedHistoryAction
    {
        [Key]
        [Required]
        public int ActionId { get; set; }
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }
    }
}
