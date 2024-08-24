
namespace OceansApp.Models.ViewModels.PaymentSheets
{
    public class GetReportToMakePaymentVM
    {
        public string ConsultantName { get; set; }
        public GetListOfMovementsForPaymentVM? ListOfMovements { get; set; }
    }
}
