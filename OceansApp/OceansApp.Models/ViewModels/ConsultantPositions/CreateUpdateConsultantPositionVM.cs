
namespace OceansApp.Models.ViewModels.ConsultantPositions
{
    public class CreateUpdateConsultantPositionVM
    {
        public int? PositionId { get; set; }
        public string? PositionName { get; set; }
        public bool? IsAdministrative { get; set; }
        public List<GetConsultantPositionConfigurationsVM> PositionConfiguration { get; set; }
    }
}
