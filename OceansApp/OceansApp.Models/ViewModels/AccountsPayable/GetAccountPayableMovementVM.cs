
namespace OceansApp.Models.ViewModels.AccountsPayable
{
    public class GetAccountPayableMovementVM
    {
        public decimal Quantity { get; set; }
        public decimal TotalAmount { get; set; }
        public string Type { get; set; }
        public int? MovementTypeId { get; set; }
        public int ProjectId { get; set; }
    }
}
