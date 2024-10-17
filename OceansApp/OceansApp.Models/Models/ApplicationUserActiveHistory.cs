
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace OceansApp.Models.Models
{
    public class ApplicationUserActiveHistory
    {
        [Key]
        [Required]
        public int HistoryId { get; set; }
        [Required]
        public bool IsActive { get; set; }
        [Required]
        [MaxLength(450)]
        public string UserId { get; set; }
        [Required]
        public DateTime ActionDate { get; set; }
        [Required]
        [MaxLength(450)]
        public string UserIdActionedBy { get; set; }

        [ValidateNever]
        public ApplicationUser ApplicationUser { get; set; }
        [ValidateNever]
        public ApplicationUser UserActionedBy { get; set; }

    }
}
