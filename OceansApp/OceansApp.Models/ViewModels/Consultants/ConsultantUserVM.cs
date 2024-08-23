
namespace OceansApp.Models.ViewModels.Consultants
{
    public class ConsultantUserVM
    {
        public int ConsultantId { get; set; }
        public string Name { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public int PaymentMethodId { get; set; }
        public string CountryId { get; set; }
        public string CompanyId { get; set; }
        public int PaymentPeriod { get; set; }
        public int? ConsultantHolidayId { get; set; }
    }
}
