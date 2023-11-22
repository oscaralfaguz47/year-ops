using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OceansApp.Models.Models
{
    public class CalculatorSearchHistory
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public DateTime SearchDate { get; set; }
        [MaxLength(35)]
        public string? SearchFrom { get; set; }
        [Required]
        [MaxLength(450)]
        public string SearchByUserId { get; set; }

        [ForeignKey("SearchByUserId")]
        [ValidateNever]
        public ApplicationUser ApplicationUser { get; set; }

    }
}
