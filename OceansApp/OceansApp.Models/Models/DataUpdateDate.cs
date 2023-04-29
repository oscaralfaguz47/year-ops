using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace OceansApp.Models.Models
{
    public class DataUpdateDate
    {
        [Required]
        [Key]
        public int IdUpdateDate { get; set; }
        [Required]
        public DateTime Date { get; set; } = DateTime.Now;
        [Required]
        [MaxLength(300)]
        public string SectionsUpdated { get; set; }
        [Required]
        [MaxLength(450)]
        public string CreatedBy { get; set; }

        [ForeignKey("CreatedBy")]
        [ValidateNever]
        public ApplicationUser ApplicationUser { get; set; }
    }
}
