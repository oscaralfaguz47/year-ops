

namespace OceansApp.Models.ViewModels.Holidays
{
    public class HolidaysGetAllWithFiltersVM
    {
        public int ConsultantHolidayId { get; set; }
        public int Year { get; set; }
        public string Name { get; set; }
        public DateTime CreationDate { get; set; }
        public string CreatedByName { get; set; }
        public int NumHolidays { get; set; }
    }
}
