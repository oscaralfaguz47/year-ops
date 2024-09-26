
using OceansApp.Models.ViewModels.ConsultantPayments;

namespace OceansApp.Models.ViewModels.PaymentSheets
{
    public class GetReportToMakePaymentVM
    {
        public string ConsultantName { get; set; }
        public decimal? AccountPayableBalance { get; set; }
        public decimal? AccountPayableAmount { get; set; }
        public bool AccountPayableIsAccounted { get; set; }
        public bool ExistsPayment { get; set; }
        public GetListOfMovementsForPaymentVM? ListOfMovements { get; set; }
        public List<GetConsultantPaymentsInPeriodVM>? PaymentsList { get; set; }
    }
}
