
namespace OceansApp.Models.ViewModels.Consultants
{
    public class ConsultantsGetAllWithFiltersVM
    {
        public int ConsultantId { get; set; }

        public string CountryName { get; set; }
        public string? Phone2 { get; set; }
        public string? Address { get; set; }
        public string? PersonalEmail { get; set; }
        public string? Location { get; set; }
        public string? ShirtSize { get; set; }
        public string ConsultantName { get; set; }
        public string InternalEmail { get; set; }
        public string? PhoneNumber { get; set; }
        public bool TwoFactorEnabled { get; set; }
        public bool EmailConfirmed { get; set; }
        public bool IsActive { get; set; }
        public bool IsLockedOut { get; set; }
        public string UserCategoryName { get; set; }
        public string? ConsultantPositions { get; set; }
        public string? ConsultantProjects { get; set; }

    }
}
