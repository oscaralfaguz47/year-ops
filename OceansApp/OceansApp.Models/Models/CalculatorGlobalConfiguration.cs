using System.ComponentModel.DataAnnotations;

namespace OceansApp.Models.Models
{
    public class CalculatorGlobalConfiguration
    {
        [Key]
        [MaxLength(25)]
        [Required]
        public String Id { get; set; } = "Configuration1";
        [Required(ErrorMessage = "La Fecha Desde es Requerida")]
        public DateTime StartDate { get; set; }
        [Required(ErrorMessage = "La Fecha Hasta es Requerida")]
        public DateTime EndDate { get; set; }
        [Required(ErrorMessage = "El número de personas es requerido")]
        [Range(1, 65, ErrorMessage = "El valor debe de ser mayor o igual a 1 y menor o igual a 65")]
        public int PeopleNumber { get; set; }
        [Required(ErrorMessage = "El número de días es requerido")]
        [Range(15, 30, ErrorMessage = "El valor debe de ser mayor o igual a 15 y menor o igual a 30")]
        public Double NumLaborDaysInMonth { get; set; }
        [Required(ErrorMessage = "El número de aumento es requerido")]
        [Range(0, 200, ErrorMessage = "El valor debe de ser mayor o igual a 0 y menor o igual a 200")]
        public Double AdditionalGlobalIncrease { get; set; }
        [Required(ErrorMessage = "El número de % es requerido")]
        public Double ProfitGreenClientAAA { get; set; }
        [Required(ErrorMessage = "El número de % es requerido")]
        public Double ProfitGreenClientAA { get; set; }
        [Required(ErrorMessage = "El número de % es requerido")]
        public Double ProfitGreenPartner { get; set; }
        [Required(ErrorMessage = "El número de % es requerido")]
        public Double ProfitYellowClientAAA { get; set; }
        [Required(ErrorMessage = "El número de % es requerido")]
        public Double ProfitYellowClientAA { get; set; }
        [Required(ErrorMessage = "El número de % es requerido")]
        public Double ProfitYellowPartner { get; set; }



    }
}
