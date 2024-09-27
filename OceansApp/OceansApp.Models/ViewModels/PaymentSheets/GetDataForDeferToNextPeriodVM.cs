
namespace OceansApp.Models.ViewModels.PaymentSheets
{
    public class GetDataForDeferToNextPeriodVM
    {
        public DateTime ActionDate { get; set; }
        List<ListOfMovementsToDeferToNextPeriodVM> listOfMovementsToDefer { get; set; }
    }
}
