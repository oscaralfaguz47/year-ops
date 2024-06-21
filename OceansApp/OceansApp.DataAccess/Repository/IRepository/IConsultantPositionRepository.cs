using OceansApp.Models.Models;
using OceansApp.Models.ViewModels.Components;
using OceansApp.Models.ViewModels.ConsultantPositions;

namespace OceansApp.DataAccess.Repository.IRepository
{
    public interface IConsultantPositionRepository : IRepository<ConsultantPosition>
    {
        Task<(List<ConsultantPositionsGetAllWithFiltersVM> positions, int totalCount)>
            GetAllConsultantPositionsWithFiltersAsync(ConsultantPositionsPaginationFiltersVM filtersAndPagination);
        Task<List<GetConsultantPositionConfigurationsVM>> GetCompanyMovementTypesByPositionIdAsync(int? positionId);
        Task<MethodResponse> CreatePositionAsync(CreateUpdateConsultantPositionVM positionConfigData);
        Task<MethodResponse> UpdatePositionAsync(CreateUpdateConsultantPositionVM positionConfigData);
        Task<List<GetDataForSelectVM>> GetPositionsByIsAdministrative(bool isAdministrative);
        Task<List<GetConsultantPostionsForSelectVM>> GetPositionsByConsultantIdAsync(int consultantId);
    }
}
