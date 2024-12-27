using OceansApp.Models.Models;
using OceansApp.Models.ViewModels.Components;
using OceansApp.Models.ViewModels.Projects;

namespace OceansApp.DataAccess.Repository.IRepository
{
    public interface IProjectRepository : IRepository<Project> 
    {
        Task<(List<ProjectsGetAllWithFiltersVM> projects, int totalCount)> GetAllProjectsWithFiltersAsync(ProjectsPaginationFiltersVM filtersAndPagination);
        Task<CreateUpdateProjectVM> GetProjectDataByIdAsync(int projectId);
        Task<GetProjectConsultantAssignedVM> GetAssignedConsultantToProjectById(int consultantProjectAssignedtId);
        Task<MethodResponse> CreateProject(CreateUpdateProjectVM projectData);
        Task<MethodResponse> UpdateProject(CreateUpdateProjectVM projectData);
        Task<MethodResponse> AddUpdateConsultantInProjet(CreateUpdateProjectConsultantHistoryVM consultantAssignationData);
        Task<List<GetDataForSelectVM>> GetAllProjectsWithActiveInactiveAsync();

    }
}
