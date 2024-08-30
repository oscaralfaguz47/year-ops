
namespace OceansApp.Models.ViewModels.ProjectConsultantAssignedHistory
{
    public class GetCurrentHistoryVM
    {
        public int PositionId { get; set; }
        public decimal MonthlySalary { get; set; }
        public decimal MonthlySalaryPartner { get; set; }
        public bool AccessToTrackingTool { get; set; }
        public bool HolidaysMustBePaid { get; set; }
        public decimal HourlyClientRate { get; set; }
        public decimal HourlySalary { get; set; }
        public bool IsActive { get; set; }
        public bool IsDefaultProject { get; set; }
        public bool IsMonthlySalaryCalculatedPerHour { get; set; }
        public decimal MonthlyClientRate { get; set; }
        public int PartnerId { get; set; }
        public bool PartnerPaysBenefits { get; set; }
    }
}
