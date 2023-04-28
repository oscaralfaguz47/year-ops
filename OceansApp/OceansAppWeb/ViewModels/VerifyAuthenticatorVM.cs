using System.ComponentModel.DataAnnotations;

namespace OceansAppWeb.ViewModels
{
    public class VerifyAuthenticatorVM
    {
        [Required]
        public string Code { get; set; }
        public string? ReturnUrl { get; set; }
        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }
}
