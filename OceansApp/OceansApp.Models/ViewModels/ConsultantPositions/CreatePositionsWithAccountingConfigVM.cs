
namespace OceansApp.Models.ViewModels.ConsultantPositions
{
    public class CreatePositionsWithAccountingConfigVM
    {
        public string Name { get; set; }
        public bool IsAdministrative { get; set; }
        public List<CreateAccountingConfigVM> AccountingConfig { get; set; }
    }
}
