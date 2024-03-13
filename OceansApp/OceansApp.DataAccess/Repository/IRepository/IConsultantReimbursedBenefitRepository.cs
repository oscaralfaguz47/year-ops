using OceansApp.Models.Models;
using OceansApp.Models.ViewModels.ConsultantReimbursedBenefits;

namespace OceansApp.DataAccess.Repository.IRepository
{
    public interface IConsultantReimbursedBenefitRepository : IRepository<ConsultantReimbursedBenefit> 
    {
        Task<(List<ConsultantReimbursedBenefitsGetAllWithFiltersVM> reimbursedBenefits, int totalCount)> GetAllConsultantsReimbursedBenefitsWithFiltersAsync(ConsultantReimbursedBenefitsPaginationFiltersVM filtersAndPagination);
        void Update(ConsultantReimbursedBenefit obj);

    }
}
