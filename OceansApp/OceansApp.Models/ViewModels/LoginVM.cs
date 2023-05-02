using System.ComponentModel.DataAnnotations;

namespace OceansApp.Models.ViewModels
{
    public class LoginVM
    {
        [Required(ErrorMessage = "El correo es requerido")]
        [Display(Name = "Correo")]
        [EmailAddress]
        public string Email { get; set; }

        [Required(ErrorMessage = "La contraseña es requerida")]
        [Display(Name = "Contraseña")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "¿Recordarme?")]
        public bool RememberMe { get; set; }
    }
}
