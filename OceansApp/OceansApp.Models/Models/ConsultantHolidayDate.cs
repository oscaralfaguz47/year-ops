
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OceansApp.Models.Models
{
    public class ConsultantHolidayDate
    {
       [Key]
       [Required]
        public int ConsultantHolidayDateId { get; set; }
        [Required]
        [ForeignKey("ConsultantHolidayId")]
        public int ConsultantHolidayId { get; set; }
        [Required]
        [MaxLength(70)]
        public string Name { get; set; }
        [Required]
        public DateTime Date { get; set; }
        [Required]
        public DateTime CreationDate { get; set; }
        [Required]
        [MaxLength(450)]
        [ForeignKey("Id")]
        public string CreatedBy { get; set; }
        public DateTime? DateLastUpdate { get; set; }
        [MaxLength(450)]
        [ForeignKey("Id")]
        public string? UpdatedBy { get; set; }

        [ValidateNever]
        public ConsultantHoliday? ConsultantHoliday { get; set; }
        [ValidateNever]
        public ApplicationUser? ApplicationUserCreated { get; set; }
        [ValidateNever]
        public ApplicationUser? ApplicationUserUpdated { get; set; }
    }
}
