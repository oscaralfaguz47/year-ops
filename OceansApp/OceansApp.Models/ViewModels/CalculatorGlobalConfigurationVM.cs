using OceansApp.Models.Models;
using System.Collections.ObjectModel;

namespace OceansApp.Models.ViewModels
{
    public class CalculatorGlobalConfigurationVM
    {
        public CalculatorGlobalConfiguration CalculatorGlobalConfiguration { get; set; }
        public Collection<CalculatorCostCenterIncreaseConfigurationVM>? CalculatorCostCenterIncreaseConfigurationVM { get; set; }
    }
}
