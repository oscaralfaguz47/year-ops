using System.ComponentModel.DataAnnotations;

namespace OceansApp.Models.Models
{
    public class SystemArea
    {
        [Key]
        public int SystemAreaId { get; set; }
        [MaxLength(50)]
        public string Name { get; set; }
    }
}
