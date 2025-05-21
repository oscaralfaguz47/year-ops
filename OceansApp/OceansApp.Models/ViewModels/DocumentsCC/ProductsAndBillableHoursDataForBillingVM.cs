

namespace OceansApp.Models.ViewModels.DocumentsCC
{
    public class ProductsAndBillableHoursDataForBillingVM
    {
        public int ProductId { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public string ProductAlias { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TaxPercentage { get; set; }
    }
}
