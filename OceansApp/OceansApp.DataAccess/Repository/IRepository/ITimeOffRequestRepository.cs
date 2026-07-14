using OceansApp.Models.Models;
using OceansApp.Models.ViewModels.Components;
using OceansApp.Models.ViewModels.TimeOff;

namespace OceansApp.DataAccess.Repository.IRepository
{
    public interface ITimeOffRequestRepository : IRepository<TimeOffRequest>
    {
        Task<(List<TimeOffRequestListVM> requests, int totalCount)> GetConsultantRequestsAsync(
            int consultantId, TimeOffPaginationFiltersVM filtersAndPagination);

        Task<(List<TimeOffRequestListVM> requests, int totalCount)> GetManagerRequestsAsync(
            int managerConsultantId, TimeOffPaginationFiltersVM filtersAndPagination);

        Task<List<TimeOffCalendarEntryVM>> GetCalendarEntriesAsync(
            int consultantId, DateTime monthStart, DateTime monthEnd);

        Task<TimeOffBalancesVM> GetBalancesAsync(int consultantId);

        Task<MethodResponse> SubmitRequestAsync(
            string userIdCreatedBy, int consultantId, SubmitTimeOffRequestVM data, string baseUrl);

        Task<MethodResponse> ApproveRejectRequestAsync(
            string userIdActionedBy, ApproveRejectTimeOffVM data, string baseUrl);

        Task<TimeOffWidgetVM> GetWidgetDataAsync(int consultantId);

        Task<List<TimeOffRequestListVM>> GetTeamWidgetDataAsync(int managerConsultantId);

        Task<List<TimeOffRequestListVM>> GetAllConsultantRequestsAsync(int consultantId);

        Task<List<DateTime>> GetConsultantHolidayDatesAsync(int consultantId);

        Task<TimeOffBalancesVM> GetAdminBalancesAsync(int consultantId);

        Task<(List<TimeOffRequestListVM> requests, int totalCount)> GetAdminApprovalsAsync(
            string approverUserId, TimeOffPaginationFiltersVM filtersAndPagination);
    }
}
