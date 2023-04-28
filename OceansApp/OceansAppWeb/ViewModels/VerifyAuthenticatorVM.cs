using System.ComponentModel.DataAnnotations;

namespace OceansAppWeb.ViewModels
{
    public class VerifyAuthenticatorVM
    {
        [Required(ErrorMessage = "El código es requerido")]
        public string Code { get; set; }
        public string? ReturnUrl { get; set; }
        [Display(Name = "¿Recordarme?")]
        public bool RememberMe { get; set; }
    }
}
