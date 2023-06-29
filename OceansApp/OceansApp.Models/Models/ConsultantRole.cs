using System.ComponentModel.DataAnnotations;


namespace OceansApp.Models.Models
{
    public class ConsultantRole
    {
        [Key]
        public int ConsultantRoleId { get; set; }
        [Required]
        [MaxLength(150)]
        public string Name { get; set; }
    }
}
