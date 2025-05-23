

namespace OceansApp.Models.ViewModels.Products
{
    public class GetProductsListWithAccountingClientStatusVM
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductCode { get; set; }
        public decimal TaxPercentage { get; set; }
        public bool ClientHasAccountingConfig { get; set; }
    }
}
