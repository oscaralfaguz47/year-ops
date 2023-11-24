using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace OceansApp.Models.Models
{
    public class ApplicationUserClaim: IdentityUserClaim<string>
    {
        public DateTime CreationDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        [MaxLength(450)]
        public string? CreatedBy { get; set; }

        [MaxLength(450)]
        public string? UpdatedBy { get; set; }
    }
}
