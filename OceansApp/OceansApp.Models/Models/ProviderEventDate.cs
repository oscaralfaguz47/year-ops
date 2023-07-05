
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OceansApp.Models.Models
{
    public class ProviderEventDate
    {
        [Key]
        public int ProviderDateId { get; set; }
        public int ProviderId { get; set; }
        [ForeignKey("ProviderId")]
        [ValidateNever]
        public Provider Provider { get; set; }
        public DateTime EventDate { get; set; }
        public int ProviderEventId { get; set; }
        [ForeignKey("ProviderEventId")]
        [ValidateNever]
        public ProviderEvent ProviderEvent { get; set; }
        public string CreatedBy { get; set; }

        [ForeignKey("CreatedBy")]
        [ValidateNever]
        public ApplicationUser ApplicationUser { get; set; }
    }
}
