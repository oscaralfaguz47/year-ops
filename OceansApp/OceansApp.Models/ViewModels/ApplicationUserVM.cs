using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace OceansApp.Models.ViewModels
{
    public class ApplicationUserVM
    {
        public String Id { get; set; }
        public String Email { get; set; }
        public String? PhoneNumber { get; set; }
        public String Role { get; set; }
        public List<SelectListItem> Roles { get; set; }

        [Required(ErrorMessage = "El nombre es requerido")]
        [Display(Name = "Nombre")]
        public String Name { get; set; }
        [Required(ErrorMessage = "El apellido es requerido")]
        [Display(Name = "Apellido")]
        public String LastName { get; set; }
        [MaxLength(100)]
        public String? Ocupation { get; set; }
        public bool IsActive { get; set; }
        public bool TwoFactorEnabled { get; set; }

    }
}
