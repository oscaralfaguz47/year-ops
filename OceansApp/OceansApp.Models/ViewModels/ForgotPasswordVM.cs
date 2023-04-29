using System.ComponentModel.DataAnnotations;

namespace OceansAppWeb.ViewModels
{
    public class ForgotPasswordVM
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

    }
}
