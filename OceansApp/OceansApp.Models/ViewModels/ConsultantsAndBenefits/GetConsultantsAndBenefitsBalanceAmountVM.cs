namespace OceansApp.Models.ViewModels.ConsultantsAndBenefits
{
    public class GetConsultantsAndBenefitsBalanceAmountVM
    {
        public int? ConsultantAndBenefitId { get; set; }
        public int BenefitId { get; set; }
        public string ConsultantName { get; set; }
        public bool IsActive { get; set; }
        public string BenefitName { get; set; }
        public decimal AmountBase { get; set; }
        public decimal BalanceAmount { get; set; }

    }
}
