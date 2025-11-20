using OceansApp.Models.Models;
using OceansApp.Models.ViewModels.ConsultantsAndBenefits;

namespace OceansApp.DataAccess.Repository.IRepository
{
    public interface IConsultantAndBenefitRepository : IRepository<ConsultantAndBenefit> 
    {
        Task<ConsultantAndBenefit> CreateConsultantAndBenefitIfNotExists(int consultantId, ConsultantBenefit benefit);
        Task<decimal?> GetBenefitBalanceAmountByConsultantAsync(int consultantId, string benefitName);
        Task<(List<GetConsultantsAndBenefitsBalanceAmountVM> results, int totalCount)> GetConsultantsAndBenefitsBalanceAsync(
            ConsultantsAndBenefitsBalancePaginationFiltersVM paginationFilters);
    }
}
