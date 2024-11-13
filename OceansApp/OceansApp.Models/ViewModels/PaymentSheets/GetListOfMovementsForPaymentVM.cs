
namespace OceansApp.Models.ViewModels.PaymentSheets
{
    public class GetListOfMovementsForPaymentVM
    {
        public List<GetPaymentDetailsMovementsVM>? ProjectMovements { get; set; }
        public List<GetPaymentDetailsMovementsVM>? BenefitsAndOtherMovements { get; set; }
        public List<GetPaymentDetailsMovementsVM>? DebitsMovements { get; set; }
    }
}
