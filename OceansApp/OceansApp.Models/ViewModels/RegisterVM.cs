using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace OceansApp.Models.ViewModels
{
    public class RegisterVM
    {
        [Required(ErrorMessage = "El correo es requerido.")]
        [EmailAddress(ErrorMessage = "El correo tiene que ser un e-mail valido.")]
        [Display(Name = "Correo")]
        public string Email { get; set; }

        [Required(ErrorMessage = "La contraseña es requerida.")]
        [MaxLength(50, ErrorMessage = "La contraseña no puede tener más de 50 caracteres.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$", ErrorMessage = "La contraseña debe contener al menos 8 caracteres, una letra minúscula, una letra mayúscula, un número y un símbolo especial.")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Password { get; set; }
        [Required(ErrorMessage = "La confirmación de contraseña es requerida.")]
        [MaxLength(50, ErrorMessage = "La contraseña no puede tener más de 50 caracteres.")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirma Contraseña")]
        [Compare("Password", ErrorMessage = "La contraseña y confirma contraseña no coinciden.")]
        public string ConfirmPassword { get; set; }

        [Display(Name = "Nombre")]
        [Required(ErrorMessage = "El nombre es requerido.")]
        [MaxLength(150, ErrorMessage = "El nombre no puede tener más de 150 caracteres.")]
        public string Name { get; set; }
        [Display(Name = "Apellido")]
        [Required(ErrorMessage = "El apellido es requerido.")]
        [MaxLength(150, ErrorMessage = "El apellido no puede tener más de 150 caracteres.")]
        public string LastName { get; set; }
        [Display(Name = "Ocupación")]
        [MaxLength(100, ErrorMessage = "La ocupación no puede tener más de 100 caracteres.")]
        public string? Occupation { get; set; }
        [Display(Name = "Teléfono")]
        [MaxLength(100, ErrorMessage = "El teléfono no puede tener más de 100 caracteres.")]
        public string? PhoneNumber { get; set; }
        [Display(Name = "Rol")]

        public List<SelectListItem> RoleList { get; set; }
        [Required(ErrorMessage = "El rol es requerido")]
        public string Role { get; set; }
    }
}
