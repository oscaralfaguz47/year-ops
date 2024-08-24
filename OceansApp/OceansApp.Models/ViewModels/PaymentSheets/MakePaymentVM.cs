
namespace OceansApp.Models.ViewModels.PaymentSheets
{
    public class MakePaymentVM
    {
        public string ConsultantName { get; set; }
        public int PaymentMethodId { get; set; }
        public string CompanyId { get; set; }
        public string CountryName { get; set; }
        public decimal AmountToPay { get; set; }
    }
}
