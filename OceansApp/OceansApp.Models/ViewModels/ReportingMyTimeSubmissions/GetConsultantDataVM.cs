
namespace OceansApp.Models.ViewModels.ReportingMyTimeSubmissions
{
    public class GetConsultantDataVM
    {
        public int ConsultantId { get; set; }
        public string ConsultantName { get; set; }
        public string Email { get; set; }
        public List<GetProjectNamesVM> Projects { get; set; }
    }
}
