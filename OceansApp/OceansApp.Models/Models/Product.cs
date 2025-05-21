
using System.ComponentModel.DataAnnotations;

namespace OceansApp.Models.Models
{
    public class Product
    {
        [Key]
        [Required]
        public int ProductId { get; set; }
        [Required]
        [MaxLength(10)]
        public required string ProductCode { get; set; }
        [MaxLength(150)]
        [Required]
        public required string Name { get; set; }
        [MaxLength(150)]
        [Required]
        public required string Alias { get; set; }
        [MaxLength(300)]
        public string? Detail { get; set; }

    }
}
