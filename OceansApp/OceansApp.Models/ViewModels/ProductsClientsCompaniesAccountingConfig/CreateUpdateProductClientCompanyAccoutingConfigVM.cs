
namespace OceansApp.Models.ViewModels.ProductsClientsCompaniesAccountingConfig
{
    public class CreateUpdateProductClientCompanyAccoutingConfigVM
    {
        public int? ProductId { get; set; }
        public int? ClientId { get; set; }
        public int? MovementTypeId { get; set; }
        public int? CostCenterIdSales { get; set; }
        public int? CostCenterIdSalesDiscount { get; set; }
        public int? CostCenterIdSalesReturn { get; set; }
        public int? CostCenterIdSalesTax { get; set; }
        public int? AccountingAccountIdSales { get; set; }
        public int? AccountingAccountIdSalesDiscount { get; set; }
        public int? AccountingAccountIdSalesReturn { get; set; }
        public int? AccountingAccountIdSalesTax { get; set; }
        public decimal? TaxPercentage { get; set; }
        public bool? IsCreating { get; set; }
    }
}
