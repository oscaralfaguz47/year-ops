
namespace OceansApp.Models.ViewModels.ConsultantPositions
{
    public class CreateUpdateConsultantPositionVM
    {
        public string? PositionName { get; set; }
        public List<GetConsultantPositionConfigurationsVM> PositionConfiguration { get; set; }
    }
}
