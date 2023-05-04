
using System.ComponentModel.DataAnnotations;

namespace OceansApp.Models.ViewModels
{
    public class TwoFactorAuthenticationVM
    {
        //used to login
        [Required(ErrorMessage = "El código es requerido")]
        public string Code { get; set; }

        //used to register / signup
        public string Token { get; set; }
        public string QRCodeUrl { get; set; }
    }
}
