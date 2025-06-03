
namespace OceansApp.Models.ViewModels.ReportingMyTimeMovements
{
    public class GetBillableHoursForBillingVM
    {
        public string ProductDescription { get; set; }
        public int MovementTypeId { get; set; }
        public decimal TotalHours { get; set; }
        public decimal UnitPrice { get; set; }
        public int? ProductIdConfigured { get; set; }
        public string? ProductCodeConfigured { get; set; }
        public int? ProductIdToConfigure { get; set; }
        public string? ProductNameToConfigure { get; set; }
        public decimal? TaxPercentage { get; set; }
    }
}
