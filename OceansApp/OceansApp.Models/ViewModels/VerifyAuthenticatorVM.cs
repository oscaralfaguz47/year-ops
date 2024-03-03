using System.ComponentModel.DataAnnotations;

namespace OceansApp.Models.ViewModels
{
    public class VerifyAuthenticatorVM
    {
        [Required(ErrorMessage = "The Code is required.")]
        [MaxLength(10, ErrorMessage = "The code cannot be more than 10 characters.")]
        [Display(Name = "Code")]
        public string Code { get; set; }
        public string? ReturnUrl { get; set; }
        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }
}
