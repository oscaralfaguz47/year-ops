using System.ComponentModel.DataAnnotations;

namespace OceansApp.Models.ViewModels.Holidays
{
    public class CreateUpdateHolidayVM
    {
        public int? ConsultantHolidayId { get; set; }
        [Required(ErrorMessage = "The year of the holiday list is required.")]
        public int Year { get; set; }
        [Required(ErrorMessage = "The Holiday name list is required.")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "The Holidays list name must be between 1 and 70 characters.")]
        public string Name { get; set; }
        public List<CreateUpdateHolidayDateVM> HolidayDates { get; set; }
        public string? CreatedBy { get; set; }
    }
}
