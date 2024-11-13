
namespace OceansApp.Models.ViewModels.PaymentSheets
{
    public class GetDataForDeferToNextPeriodVM
    {
        public DateTime ActionDate { get; set; }
        public string CompanyId { get; set; }
        public List<ListOfMovementsToDeferToNextPeriodVM> ListOfMovementsToDefer { get; set; }
    }
}
