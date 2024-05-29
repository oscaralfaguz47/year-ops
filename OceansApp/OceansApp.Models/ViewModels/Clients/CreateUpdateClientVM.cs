using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace OceansApp.Models.ViewModels.Clients
{
    public class CreateUpdateClientVM
    {
        public int? ClientId { get; set; }
        public string? Name { get; set; }
        public string? Contact { get; set; }
        public string? ContactOccupation { get; set; }
        public string? Emails { get; set; }
        public string? AdmissionDate { get; set; }
        public string? PaymentCondition { get; set; }
        public string? IsActive { get; set; }
        public string? ClientClass { get; set; }
        public string? Address { get; set; }
        public string? CompanyId { get; set; }
        public int? SuccessManagerId { get; set; }
        public string? SuccessManager { get; set; }
        public decimal? LatePaymentFee { get; set; }
        public string? AdditionalEmailsForNotifications { get; set; }
        public bool? AllowSentLatePaymentNotifications { get; set; }
    }
}
