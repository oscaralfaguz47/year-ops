using OceansApp.Models.Models;
using OceansApp.Models.ViewModels.Components;
using OceansApp.Models.ViewModels.Projects;

namespace OceansApp.DataAccess.Repository.IRepository
{
    public interface IProjectRepository : IRepository<Project> 
    {
        Task<(List<ProjectsGetAllWithFiltersVM> projects, int totalCount)> GetAllProjectsWithFiltersAsync(ProjectsPaginationFiltersVM filtersAndPagination);
        Task<CreateUpdateProjectVM> GetProjectDataById(int projectId);
        Task<MethodResponse> CreateProjectWithAssignedConsultants(CreateUpdateProjectVM projectData);
        void Update(Project obj);

    }
}
