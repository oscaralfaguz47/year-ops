using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace OceansApp.Models.Models
{
    public class ApplicationUser: IdentityUser
    {
        public string? Name { get; set; }
        public string LastName { get; set; }
        public string? Occupation { get; set; }
        public bool IsActive { get; set; }
        public DateTime? DeactivationDate { get; set; }
        public string? OpaqueToken { get; set; }
        public DateTime? OpaqueTokenExpiration { get; set; }
        [Required]
        public int UserCategoryId { get; set; }
        [Required]
        public bool TwoFactorRequired { get; set; } = true;

        [ValidateNever]
        public ApplicationUserCategory ApplicationUserCategory { get; set; }
    }
}
