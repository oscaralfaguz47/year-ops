using System.ComponentModel.DataAnnotations;

namespace OceansApp.Models.ViewModels
{
    public class VerifyAuthenticatorVM
    {
        [Required(ErrorMessage = "El código es requerido")]
        [MaxLength(10, ErrorMessage = "El código no puede tener más de 10 caracteres.")]
        [Display(Name = "Código")]
        public string Code { get; set; }
        public string? ReturnUrl { get; set; }
        [Display(Name = "¿Recordarme?")]
        public bool RememberMe { get; set; }
    }
}
