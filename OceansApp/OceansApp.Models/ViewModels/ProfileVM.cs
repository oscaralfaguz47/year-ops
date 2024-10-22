

using System.ComponentModel.DataAnnotations;

namespace OceansApp.Models.ViewModels
{
    public class ProfileVM
    {
        public string? Id { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Name { get; set; }
        public string? LastName { get; set; }
        public string? Occupation { get; set; }
        public string? ProfileUrl { get; set; }
    }
}
