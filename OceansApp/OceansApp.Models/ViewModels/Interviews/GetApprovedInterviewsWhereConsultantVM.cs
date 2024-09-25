
namespace OceansApp.Models.ViewModels.Interviews
{
    public class GetApprovedInterviewsWhereConsultantVM
    {
        public int MovementId { get; set; }
        public decimal TotalDurationHours { get; set; }
        public int MovementTypeId { get; set; }
        public string MovementTypeName { get; set; }
    }
}
