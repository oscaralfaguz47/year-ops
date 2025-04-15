
namespace OceansApp.Models.ViewModels.Projects
{
    public class GetProjectConsultantAssignedHistoryVM
    {
        public int Id { get; set; }
        public DateTime ActionDate { get; set; }
        public string PositionName { get; set; }
        public decimal? HourlySalary { get; set; }
        public decimal? MonthlySalary { get; set; }
        public bool IsMonthlySalaryCalculatedPerHour { get; set; }
        public decimal? MonthlySalaryPartner { get; set; }
        public string PartnerName { get; set; }
        public bool? PartnerPaysBenefits { get; set; }
        public decimal? HourlyClientRate { get; set; }
        public decimal? MonthlyClientRate { get; set; }
        public bool AccessToTrackingTool { get; set; }
        public bool HolidaysMustBePaid { get; set; }
        public bool IsActive { get; set; }
        public bool IsDefaultProject { get; set; }
        public string UserActionedBy { get; set; }
        public DateTime CreationDate { get; set; }
        public bool ParticipatesInOnCalls { get; set; }
        public int NumHoursForHoliday { get; set; }
    }
}
