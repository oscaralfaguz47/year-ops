using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace OceansAppWeb.Models
{
    public class ApplicationUser: IdentityUser
    {
        [Required(ErrorMessage = "El nombre es requerido")]
        [Display(Name = "Nombre")]
        [Range(1, 100, ErrorMessage = "El nombre debe de contener de 1 a 100 caractéres")]
        public String? Name { get; set; }
        [Required(ErrorMessage = "El apellido es requerido")]
        [Display(Name = "Apellido")]
        [Range(1, 100, ErrorMessage = "El apellido debe de contener de 1 a 100 caractéres")]
        public String LastName { get; set; }
        [Range(1, 100, ErrorMessage = "La ocupación debe de contener de 1 a 100 caractéres")]
        [MaxLength(100)]
        public String? Occupation { get; set; }
        public bool IsActive { get; set; }
        public DateTime? DeactivationDate { get; set; }
    }
}
