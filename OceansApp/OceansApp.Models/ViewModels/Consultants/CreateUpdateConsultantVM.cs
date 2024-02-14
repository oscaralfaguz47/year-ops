
namespace OceansApp.Models.ViewModels.Consultants
{
    public class CreateUpdateConsultantVM
    {
        //USER 
        public string? Name { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? UserCategoryName { get; set; }
        //CONSULTANT
        public int? ConsultantId { get; set; }
        public string? IdCountry { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Phone2 { get; set; }
        public string? Address { get; set; }
        public string? PersonalEmail { get; set; }
        public string? Location { get; set; }
        public string? UserRole { get; set; }
    }
}
