
namespace OceansApp.Models.ViewModels.Clients
{
    public class ClientsGetAllWithFiltersVM
    {
        public int ClientId { get; set; }
        public string Name { get; set; }
        public string? Contact { get; set; }
        public string? ContactOccupation { get; set; }
        public string? Emails { get; set; }
        public DateTime AdmissionDate { get; set; }
        public string PaymentCondition { get; set; }
        public string IsActive { get; set; }
        public string? ClientClass { get; set; }
        public string? Address { get; set; }
        public string CompanyId { get; set; }
        public string? SuccessManager { get; set; }
        public string LatePaymentFee { get; set; }
        public string? AdditionalEmailsForNotifications { get; set; }
        public bool AllowSentLatePaymentNotifications { get; set; }
    }
}
