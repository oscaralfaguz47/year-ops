
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
        public int ConsultantId { get; set; }
        public decimal? MonthlySalaryThirdParty { get; set; }
        public int? PartnerId { get; set; }
        public string? PartnerName { get; set; }
        public int PositionId { get; set; }
        public string PositionName { get; set; }
        public bool isMonthlySalaryCalculatedPerHour { get; set; }
        public string UserCategoryName { get; set; }
        public bool AccessToTrackingTool { get; set; }
        public bool IsDefaultProject { get; set; }
    }
}
