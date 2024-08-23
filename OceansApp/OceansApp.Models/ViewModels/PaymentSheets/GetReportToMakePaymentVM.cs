
namespace OceansApp.Models.ViewModels.PaymentSheets
{
    public class GetReportToMakePaymentVM
    {
        public string ConsultantName { get; set; }
        public string CompanyId { get; set; }
        public int PaymentMethodId { get; set; }
        public string CountryId { get; set; }
        public GetListOfMovementsForPaymentVM? ListOfMovements { get; set; }
    }
}
