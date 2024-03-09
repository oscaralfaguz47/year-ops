
namespace OceansApp.Models.ViewModels.Projects
{
    public class CreateUpdateProjectConsultantAssignedVM
    {
        public int? ProjectConsultantAssignedId { get; set; }
        public int ConsultantId { get; set; }
        public string? ConsultantName { get; set; }
        public decimal? HourlyClientRate { get; set; }
        public decimal? HourlySalary { get; set; }
        public decimal? MonthlyClientRate { get; set; }
        public decimal? MonthlySalary { get; set; }
        public string PositionDetail { get; set; }
        public string? ActionDate { get; set; }
        public bool? IsActive { get; set; }
        public bool? isMonthlySalaryCalculatedPerHour { get; set; }
        public string? UserCategoryName { get; set; }
    }
}
