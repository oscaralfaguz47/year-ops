using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OceansApp.Models.Models
{
    public class DataFromSoftland
    {
        [Display(Name = "Datos a Actualizar")]
        [NotMapped]
        [Required(ErrorMessage ="El Objeto JSON es requerido")]
        public string DataToSave { get; set; }
    }
}
