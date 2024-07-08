
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OceansApp.Models.Models
{
    public class ConsultantHoliday
    {
        [Key]
        [Required]
        public int ConsultantHolidayId { get; set; }
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }
        [Required]
        public DateTime CreationDate { get; set; }
        [Required]
        [MaxLength(450)]
        [ForeignKey("Id")]
        public string CreatedBy { get; set; }


        [ValidateNever]
        public ApplicationUser ApplicationUser { get; set; }

    }
}
