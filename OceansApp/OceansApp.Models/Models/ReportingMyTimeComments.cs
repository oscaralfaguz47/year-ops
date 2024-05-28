
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace OceansApp.Models.Models
{
    public class ReportingMyTimeComments
    {
        [Required]
        [Key]
        public int CommentId { get; set; }
        [Required]
        public string Body { get; set; }
        [Required]
        public DateTime CreationDate { get; set; }
        public DateTime? LastUpdatedDate { get; set; }
        [MaxLength(450)]
        public string UserId { get; set; }
        [Required]
        public int ProjectId { get; set; }
        [Required]
        public int ConsultantId { get; set; }
        [Required]
        public DateTime ActionDate { get; set; }
        public int? SubmissionId { get; set; }

        [ValidateNever]
        public ApplicationUser ApplicationUser { get; set; }
        [ValidateNever]
        public Project Project { get; set; }
        [ValidateNever]
        public ConsultantDetail ConsultantDetail { get; set; }
        [ValidateNever]
        public ReportingMyTimeMovementSubmission ReportingMyTimeMovementSubmission { get; set; }
    }
}
