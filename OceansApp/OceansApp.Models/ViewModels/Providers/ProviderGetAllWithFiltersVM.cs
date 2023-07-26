

using OceansApp.Utility;

namespace OceansApp.Models.ViewModels.Providers
{
    public class ProviderGetAllWithFiltersVM
    {

        public string Name { get; set; }
        public string? Alias { get; set; }

        public string Occupation { get; set; }
        public string? Address { get; set; }
        public string? Email { get; set; }
        public DateTime AdmissionDate { get; set; }

        public string? Phone1 { get; set; }

        public string? Phone2 { get; set; }

        public string CountryName { get; set; }

        public string? Notes { get; set; }
        public string IsActive { get; set; }
        public string CategoryDescription { get; set; }

        public string CompanyId { get; set; }

        public string ClientName { get; set; }
        public string? PersonalEmail { get; set; }
        public string? ConsultantCategory { get; set; }
        public decimal? HourlySalary { get; set; }
        public decimal? MonthlySalary { get; set; }
        public decimal? HourlyClientRate { get; set; }
        public decimal? MonthlyClientRate { get; set; }
        public string? Location { get; set; }
        public string? ShirtSize { get; set; }

    }
}
