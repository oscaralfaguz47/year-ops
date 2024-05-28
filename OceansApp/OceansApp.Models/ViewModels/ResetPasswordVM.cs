using System.ComponentModel.DataAnnotations;

namespace OceansApp.Models.ViewModels
{
    public class ResetPasswordVM
    {
        [Required(ErrorMessage = "The Email is required.")]
        [EmailAddress(ErrorMessage = "The email must be a valid email.")]
        [Display(Name = "Correo")]
        public string Email { get; set; }

        [Required(ErrorMessage = "The Password is required.")]
        [MaxLength(50, ErrorMessage = "Password cannot be more than 50 characters.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$", ErrorMessage = "The password must contain at least 8 characters, a lowercase letter, an uppercase letter, a number, and a special symbol.")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Password Confirmation is required.")]
        [DataType(DataType.Password)]
        [Display(Name = "Password Confirmation")]
        [Compare("Password", ErrorMessage = "The Password and Password Confirmation do not match.")]
        public string ConfirmPassword { get; set; }

        public string Code { get; set; }
    }
}
