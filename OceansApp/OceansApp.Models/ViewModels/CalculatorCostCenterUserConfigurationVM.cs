namespace OceansApp.Models.ViewModels
{
    public class CalculatorCostCenterUserConfigurationVM
    {
        public int CostCenterId { get; set; }
        public string Description { get; set; }
        public string? Detail { get; set; }

        public bool Active { get; set; }

    }
}
