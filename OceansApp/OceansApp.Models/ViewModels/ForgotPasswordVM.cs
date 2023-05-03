using System.ComponentModel.DataAnnotations;

namespace OceansApp.Models.ViewModels
{
    public class ForgotPasswordVM
    {
        [Required(ErrorMessage = "El correo es requerido.")]
        [MaxLength(256, ErrorMessage = "El correo no puede tener más de 256 caracteres")]
        [EmailAddress(ErrorMessage = "El correo debe de ser un e-mail valido.")]
        [Display(Name = "Correo")]
        public string Email { get; set; }

    }
}
