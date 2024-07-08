
namespace OceansApp.Models.ViewModels.Holidays
{
    public class CreateUpdateHolidayVM
    {
        public int? ConsultantHolidayId { get; set; }
        public string? Name { get; set; }
        public List<CreateUpdateHolidayDateVM>? HolidayDates { get; set; }
        public string? CreatedBy { get; set; }
    }
}
