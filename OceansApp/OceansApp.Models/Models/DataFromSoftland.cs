using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OceansApp.Models.Models
{
    public class DataFromSoftland
    {
        [Display(Name = "Update Data")]
        [NotMapped]
        [Required(ErrorMessage ="The JSON object is required")]
        public string DataToSave { get; set; }
    }
}
