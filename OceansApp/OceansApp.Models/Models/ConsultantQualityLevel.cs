using System.ComponentModel.DataAnnotations;

namespace OceansApp.Models.Models
{
    public class ConsultantQualityLevel
    {
        [Key]
        public int ConsultantQualityLevelId { get; set; }
        [Required]
        [MaxLength(150)]
        public string Name { get; set; }
    }
}
