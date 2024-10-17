using OceansApp.Models.Models;

namespace OceansApp.DataAccess.Repository.IRepository
{
    public interface IConsultantAndBenefitRepository : IRepository<ConsultantAndBenefit> 
    {
        Task<ConsultantAndBenefit> CreateConsultantAndBenefitIfNotExists(int consultantId, ConsultantBenefit benefit);
        Task<decimal?> GetBenefitBalanceAmountByConsultantAsync(int consultantId, string benefitName);
    }
}
