
namespace OceansApp.Models.ViewModels
{
    public class CalculatorExpensesCostsDistribution
    {
        public String IdAccountingAccount { get; set; }
        public String AccountingAccountName { get; set; }
        public Decimal Amount { get; set; }
        public String CostCenterName { get; set; }
        public Decimal increasePercentage { get; set; }
        public Decimal increaseAmount { get; set; }
    }
}
