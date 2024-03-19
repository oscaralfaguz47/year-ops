using OceansApp.Models.Models;
using OceansApp.Models.ViewModels.Components;
using OceansApp.Models.ViewModels.ConsultantReimbursedBenefits;

namespace OceansApp.DataAccess.Repository.IRepository
{
    public interface IConsultantReimbursedBenefitRepository : IRepository<ConsultantReimbursedBenefit>
    {
        Task<(List<ConsultantReimbursedBenefitsGetAllWithFiltersVM> reimbursedBenefits, int totalCount)> GetAllConsultantsReimbursedBenefitsWithFiltersAsync(ConsultantReimbursedBenefitsPaginationFiltersVM filtersAndPagination);
        Task<MethodResponse> CreateBenefitReimbursement(string userIdCreatedBy, DateTime timeZone,
            CreateUpdateConsultantBenefitReimbursementVM benefitReimbursementData);
        Task<MethodResponse> UpdateBenefitReimbursement(string userActionedBy, DateTime timeZone,
            CreateUpdateConsultantBenefitReimbursementVM benefitReimbursementData);
        Task<CreateUpdateConsultantBenefitReimbursementVM> GetBenefitReimbursementDataById(int benefitReimbursementId);
        Task<MethodResponse> DeleteBenefitReimbursement(int benetifReimbursementId);
        Task<GetConsumedAmountVM> GetConsumedAmountPerYearByConsultant(int consultantId, int benefitId, int year,
            decimal amountToBeReimbursed, int? reimbursedBenefitIdToIgnore);
    }
}
