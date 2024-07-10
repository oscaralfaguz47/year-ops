
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
        public decimal? MonthlySalaryThirdParty { get; set; }
        public int? PartnerId { get; set; }
        public int? PositionId { get; set; }
        public string? ActionDate { get; set; }
        public string? StatusAction { get; set; }
        public bool? IsMonthlySalaryCalculatedPerHour { get; set; }
        public string? UserCategoryName { get; set; }
        public bool? AccessToTrackingTool { get; set; }
        public bool IsDefaultProject { get; set; }
    }
}
