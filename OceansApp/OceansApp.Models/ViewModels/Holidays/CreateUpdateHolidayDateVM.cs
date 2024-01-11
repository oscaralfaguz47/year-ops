
using System.ComponentModel.DataAnnotations;

namespace OceansApp.Models.ViewModels.Holidays
{
    public class CreateUpdateHolidayDateVM
    {
        public int? ConsultantHolidayDateId { get; set; }
        [Required(ErrorMessage = "The Holiday name is required.")]
        [StringLength(70, MinimumLength = 1, ErrorMessage = "Holiday name must be between 1 and 100 characters.")]
        public string Name { get; set; }
        [Required(ErrorMessage = "The Holiday date is required.")]
        public DateTime Date { get; set; }
    }
}
