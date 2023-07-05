
using System.ComponentModel.DataAnnotations;

namespace OceansApp.Models.ViewModels
{
    public class CalculatorCostCenterIncreaseConfigurationVM
    {
        public int? CostCenterId { get; set; }
        public string? Description { get; set; }
        [Required(ErrorMessage = "El número de porcentage es requerido")]
        public Double? Increase { get; set; }
        public string? CompanyId { get; set; }

    }
}
