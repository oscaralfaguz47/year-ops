using OceansApp.Models.Models;
using OceansApp.Models.ViewModels.Components;
using OceansApp.Models.ViewModels.ConsultantReimbursedBenefits;
using OceansApp.Models.ViewModels.Dashboard;
using System.Data;

namespace OceansApp.DataAccess.Repository.IRepository
{
    public interface IConsultantReimbursedBenefitRepository : IRepository<ConsultantReimbursedBenefit>
    {
        Task<(List<ConsultantReimbursedBenefitsGetAllWithFiltersVM> reimbursedBenefits, int totalCount)> GetAllConsultantsReimbursedBenefitsWithFiltersAsync(ConsultantReimbursedBenefitsPaginationFiltersVM filtersAndPagination);
        Task<MethodResponse> CreateBenefitReimbursement(string userIdCreatedBy,
            CreateUpdateConsultantBenefitReimbursementVM benefitReimbursementData);
        Task<MethodResponse> UpdateBenefitReimbursement(string userActionedBy,
            CreateUpdateConsultantBenefitReimbursementVM benefitReimbursementData);
        Task<CreateUpdateConsultantBenefitReimbursementVM> GetBenefitReimbursementDataById(int benefitReimbursementId);
        Task<MethodResponse> RejectBenefitReimbursement(string userActionedBy, int benetifReimbursementId);
        Task<GetConsumedAmountVM> GetConsumedAmountPerYearByConsultant(int consultantId, int benefitId, int year,
            decimal amountToBeReimbursed, int? reimbursedBenefitIdToIgnore, IDbTransaction transaction = null);
        Task<List<GetApprovedBenefitsWhereConsultant>> GetApprovedBenefitsWhereConsultantInThePeriod(int consultantId,
          DateTime startDate, DateTime endDate);
        Task<List<BenefitLastRequestsVM>> GetLastBenefitRequests(int consultantId, string benefitName);
    }
}
