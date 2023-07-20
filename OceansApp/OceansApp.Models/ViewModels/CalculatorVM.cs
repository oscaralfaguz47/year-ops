using Microsoft.AspNetCore.Mvc.Rendering;
using OceansApp.Models.Models;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

namespace OceansApp.Models.ViewModels
{
    public class CalculatorVM
    {
        [Range(0, 20, ErrorMessage = "El valor debe de ser mayor o igual a 0 y menor o igual a 20")]
        public Double? DaysYear { get; set; } = 7;
        [Range(0, 20, ErrorMessage = "El valor debe de ser mayor o igual a 0 y menor o igual a 20")]
        public Double? VacationDays { get; set; } = 0;
        [Required(ErrorMessage = "La expectativa del consultor es requerido")]
        [Range(500, 50000, ErrorMessage = "El valor debe de ser mayor o igual a $500 y menor o igual a $50,000")]
        public Double? PaymentAmount { get; set; } = null;
        [Required(ErrorMessage = "El cliente es requerido")]
        public String Client { get; set; }
        [Required(ErrorMessage = "El puesto del consultor es requerido")]
        public String ConsultantRoleId { get; set; }
        [Required(ErrorMessage = "La clasificación del consultor es requerido")]
        public String ConsultantQualityLevelId { get; set; }
        public List<SelectListItem>? ClientList { get; set; }
        public List<SelectListItem>? ConsultantRoleList { get; set; }
        public List<SelectListItem>? ConsultantQualityLevelList { get; set; }
        public Decimal MinProfitSetPercentage { get; set; }
        public Decimal MaxProfitSetPercentage { get; set; }
        public Decimal GreenPercentageInResults { get; set; } = 0;

        public CalculatorCostCenterIncreaseConfiguration? CalculatorCostCenterIncreaseConfiguration { get; set; }
        public Collection<CalculatorCostCenterUserConfigurationVM>? CalculatorCostCenterUserConfigurationVM { get; set; }
        public List<CalculatorExpensesCostsDistribution>? CalculatorExpensesCostsDistribution { get; set; }
    }
}
