
namespace OceansApp.Models.ViewModels.Projects
{
    public class CreateUpdateProjectConsultantHistoryVM
    {
        public bool? IsAssigningFirstTime { get; set; }
        public int? ConsultantId { get; set; }
        public int? ProjectId { get; set; }
        public decimal? HourlyClientRate { get; set; }
        public decimal? HourlySalary { get; set; }
        public decimal? MonthlyClientRate { get; set; }
        public decimal? MonthlySalary { get; set; }
        public decimal? MonthlySalaryPartner { get; set; }
        public int? PartnerId { get; set; }
        public bool? PartnerPaysBenefits { get; set; }
        public bool? HolidaysMustBePaid { get; set; }
        public int? PositionId { get; set; }
        public DateTime? ActionDate { get; set; }
        public bool? IsMonthlySalaryCalculatedPerHour { get; set; }
        public bool? AccessToTrackingTool { get; set; }
        public bool? IsDefaultProject { get; set; }
        public string? UserCreatedBy { get; set; }
    }
}
