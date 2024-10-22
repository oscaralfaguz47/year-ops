

using System.ComponentModel.DataAnnotations;

namespace OceansApp.Models.ViewModels
{
    public class ProfileVM
    {
        public String Id { get; set; }
        public String? Email { get; set; }
        public String? PhoneNumber { get; set; }

        [Required(ErrorMessage = "The Name is required")]
        [Display(Name = "Name")]
        public String Name { get; set; }
        [Required(ErrorMessage = "The Last Name is required")]
        [Display(Name = "Last Name")]
        public String LastName { get; set; }
        [MaxLength(100)]
        public String? Ocupation { get; set; }
        public string? ProfileUrl { get; set; }
    }
}
