
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace OceansApp.Models.Models
{
    public class ReportingMyTimeMovementBlob
    {
        [Required]
        [Key]
        public int InternalBlobId { get; set; }
        [Required]
        public int MovementId { get; set; }
        [Required]
        [MaxLength(1024)]
        public string BlobName { get; set; }
        [Required]
        [MaxLength(255)]
        public string ContainerId { get; set; }
        [Required]
        [MaxLength(2000)]
        public string BlobUrl { get; set; }
        [Required]
        public long Size { get; set; }
        [Required]
        [StringLength(255)]
        public string ContentType { get; set; }

        [Required]
        public DateTime CreationDate { get; set; }

        [ValidateNever]
        public ReportingMyTimeMovement ReportingMyTimeMovement { get; set; } 

    }
}
