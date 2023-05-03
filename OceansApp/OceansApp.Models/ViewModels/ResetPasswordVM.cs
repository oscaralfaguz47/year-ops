using System.ComponentModel.DataAnnotations;

namespace OceansApp.Models.ViewModels
{
    public class ResetPasswordVM
    {
        [Required(ErrorMessage = "El correo es requerido.")]
        [EmailAddress(ErrorMessage = "El correo debe ser un e-mail valido.")]
        [Display(Name = "Correo")]
        public string Email { get; set; }

        [Required(ErrorMessage = "La contraseña es requerida.")]
        [MaxLength(50, ErrorMessage = "La contraseña no puede tener más de 50 caracteres.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$", ErrorMessage = "La contraseña debe contener al menos 8 caracteres, una letra minúscula, una letra mayúscula, un número y un símbolo especial.")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Password { get; set; }

        [Required(ErrorMessage = "La confirmación de contraseña es requerida.")]
        [MinLength(8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres.")]
        [MaxLength(50, ErrorMessage = "La contraseña no puede tener más de 50 caracteres.")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirma Contraseña")]
        [Compare("Password", ErrorMessage = "La contraseña y confirma contraseña no coinciden.")]
        public string ConfirmPassword { get; set; }

        public string Code { get; set; }
    }
}
