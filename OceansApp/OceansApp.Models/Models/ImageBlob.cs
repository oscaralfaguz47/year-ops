
using System.ComponentModel.DataAnnotations;

namespace OceansApp.Models.Models
{
    public class ImageBlob
    {
        [Key]
        [Required]
        public int BlobId { get; set; }
        [Required]
        [MaxLength(255)]
        public string BlobName { get; set; }
        [Required]
        [MaxLength(255)]
        public string ContainerName { get; set; }
        [Required]
        public string BlobUrl { get; set; }
        [Required]
        public DateTime CreationDate { get; set; }
        [Required]
        [MaxLength(450)]
        public string EntityId { get; set; }
        [Required]
        [MaxLength(30)]
        public string EntityType { get; set; }

    }
}
