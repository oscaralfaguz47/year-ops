
namespace OceansApp.Models.ViewModels.PaymentSheets
{
    public class GetReportToMakePaymentVM
    {
        public string ConsultantName { get; set; }
        public string CompanyId { get; set; }
        public int PaymentMethodId { get; set; }
        public string CountryId { get; set; }
        public List<GetPaymentDetailsMovementsVM>? ProjectMovements { get; set; }
        public List<GetPaymentDetailsMovementsVM>? BenefitsAndOtherMovements { get; set; }
        public List<GetPaymentDetailsMovementsVM>? DebitsMovements { get; set; }
    }
}
