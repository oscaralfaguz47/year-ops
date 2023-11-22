using OceansApp.Models.Models;
using OceansApp.Models.ViewModels.ConsultantRolesQualityLevels;
using System.Collections.ObjectModel;

namespace OceansApp.Models.ViewModels
{
    public class CalculatorGlobalConfigurationVM
    {
        public CalculatorGlobalConfiguration CalculatorGlobalConfiguration { get; set; }
        public Collection<CalculatorCostCenterIncreaseConfigurationVM>? CalculatorCostCenterIncreaseConfigurationVM { get; set; }
        public List<GetConsultantRolesQualityLevelsVM> ConsultantRolesQualityLevels { get; set; }
        public List<ConsultantRole>? ConsultantRolesList { get; set; }
        public List<ConsultantQualityLevel>? ConsultantQualityLevelsList { get; set; }
        public List<ConsultantSeniority>? ConsultantSenioritisList { get; set; }
    }
}
