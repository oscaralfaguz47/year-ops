using System.ComponentModel.DataAnnotations;

namespace OceansApp.Models.ViewModels
{
    public class ForgotPasswordVM
    {
        [Required(ErrorMessage = "The Email is required.")]
        [MaxLength(256, ErrorMessage = "The email cannot be more than 256 characters")]
        [EmailAddress(ErrorMessage = "The email must be a valid email.")]
        [Display(Name = "Email")]
        public string Email { get; set; }

    }
}
