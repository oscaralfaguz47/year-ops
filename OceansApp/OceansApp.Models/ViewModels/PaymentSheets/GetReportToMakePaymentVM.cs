
using OceansApp.Models.ViewModels.ConsultantPayments;

namespace OceansApp.Models.ViewModels.PaymentSheets
{
    public class GetReportToMakePaymentVM
    {
        public string ConsultantName { get; set; }
        public GetListOfMovementsForPaymentVM? ListOfMovements { get; set; }
        public List<GetConsultantPaymentsInPeriodVM>? PaymentsList { get; set; }
    }
}
