
namespace OceansApp.Models.ViewModels.ProjectConsultantAssigned
{
    public class GetProjectInfoWhereConsultantIsActiveInProjectVM
    {
        public int ProjectId { get; set; }
        public string ProjectName { get; set; }
        public decimal MonthlySalary { get; set; }
        public decimal HourlySalary { get; set; }
        public decimal MonthlySalaryPartner { get; set; }
        public bool HolidaysMustBePaid { get; set; }
        public bool IsDefaultProject { get; set; }
        public bool IsMonthlySalaryCalculatedPerHour { get; set; }
        public int? PartnerId { get; set; }
        public bool AccessToTrackingTool { get; set; }
        public string? PartnerName { get; set; }
        public bool PartnerPaysBenefits { get; set; }
    }
}
