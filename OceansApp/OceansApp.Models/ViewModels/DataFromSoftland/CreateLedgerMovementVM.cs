
namespace OceansApp.Models.ViewModels.DataFromSoftland
{
    public class CreateLedgerMovementVM
    {
        public string IdSeat { get; set; }

        public int Consecutive { get; set; }
        public string CostCenterCode { get; set; }
        public string AccountingAccountCode { get; set; }

        public DateTime Date { get; set; }
        public Decimal LocalDebit { get; set; }
        public Decimal LocalCredit { get; set; }
        public string AccountingType { get; set; }
        public DateTime RecordDate { get; set; }
        public string? CompanyId { get; set; }
    }
}
