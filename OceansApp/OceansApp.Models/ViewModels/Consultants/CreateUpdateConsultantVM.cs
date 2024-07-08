
using OceansApp.Models.Models;
using OceansApp.Models.ViewModels.AdminCenter.ConsultantPositions;

namespace OceansApp.Models.ViewModels.Consultants
{
    public class CreateUpdateConsultantVM
    {
        //USER 
        public string? Name { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public int? UserCategoryId { get; set; }
        public string? UserCategoryName { get; set; }
        //CONSULTANT
        public int? ConsultantId { get; set; }
        public string? IdCountry { get; set; }
        public string? CompanyId { get; set; }
        public int? PaymentMethodId { get; set; }
        public string? PaymentMethodName { get; set; }
        public string? CountryName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Phone2 { get; set; }
        public string? Address { get; set; }
        public string? PersonalEmail { get; set; }
        public string? Location { get; set; }
        public string? UserRole { get; set; }
        public int? PaymentPeriod { get; set; }
        public bool ParticipatesInOnCalls { get; set; } = false;
        public int? ConsultantHolidayId { get; set; }
        public string? ConsultantHolidayName { get; set; }
        public int? PartnerId { get; set; }
        public string? PartnerName { get; set; }
        public List<CreateUpdateConsultantsAndPositionsVM>? Positions { get; set; }
    }
}
