
namespace OceansApp.Models.ViewModels.ApplicationUser
{
    public class UserAndConsultantVM
    {
        public string UserId { get; set; }
        public string Name { get; set; }
        public string LastName { get; set; }
        public int? ConsultantHolidayId { get; set; }
        public int? WorkingModel { get; set; }
        public DateTime? StartDate { get; set; }
        public string UserCategoryName { get; set; }
    }
}
