
namespace OceansApp.Models.ViewModels.ReportingMyTime
{
    public class GetProjectMovementsVM
    {
        public int MovementId { get; set; }
        public string MovementTypeName { get; set; }
        public decimal Quantity { get; set; }
        public string Notes { get; set; }
        public DateTime ActionDate { get; set; }
        public string TransactionStatus { get; set; }
        public string BlobNames { get; set; } 
    }
}