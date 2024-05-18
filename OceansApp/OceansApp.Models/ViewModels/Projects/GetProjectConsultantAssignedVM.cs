
namespace OceansApp.Models.ViewModels.Projects
{
    public class GetProjectConsultantAssignedVM
    {
        public string ConsultantName { get; set; }
        public string Email { get; set; }
        public decimal? HourlyClientRate { get; set; }
        public decimal? HourlySalary { get; set; }
        public decimal? MonthlyClientRate { get; set; }
        public decimal? MonthlySalary { get; set; }
        public decimal? MonthlySalaryThirdParty { get; set; }
        public string PositionDetail { get; set; }
        public bool isMonthlySalaryCalculatedPerHour { get; set; }
        public string UserCategoryName { get; set; }
    }
}
