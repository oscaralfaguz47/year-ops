using System.ComponentModel.DataAnnotations;

namespace OceansApp.Models.Models
{
    public class ConsultantSeniority
    {
        [Key]
        public int ConsultantSeniorityId { get; set; }
        [Required]
        [MaxLength(150)]
        public string Name { get; set; }
    }
}
