using System.ComponentModel.DataAnnotations;

namespace OceansApp.Models.ViewModels
{
    public class LoginVM
    {
        [Required(ErrorMessage = "The Email is required.")]
        [Display(Name = "Email")]
        [EmailAddress(ErrorMessage = "The email must be a valid email.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "The Password is required.")]
        [Display(Name = "Password")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Remember me on this device")]
        public bool RememberMe { get; set; }
    }
}
