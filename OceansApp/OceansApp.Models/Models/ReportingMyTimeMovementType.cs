
using System.ComponentModel.DataAnnotations;

namespace OceansApp.Models.Models
{
    public class ReportingMyTimeMovementType
    {
        [Key]
        [Required]
        public int MovementTypeId { get; set; }
        [Required]
        [MaxLength(80)]
        public string Name { get; set; }
    }
}
