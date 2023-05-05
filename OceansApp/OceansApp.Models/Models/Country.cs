using System.ComponentModel.DataAnnotations;

namespace OceansApp.Models.Models
{
    public class Country
    {
        [Key]
        [MaxLength(4)]
        [Required]
        public string IdCountry { get; set; }
        [MaxLength(40)]
        [Required]
        public string Name{ get; set; }
        [Required]
        public DateTime CreateDate { get; set; }

    }
}
