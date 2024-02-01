using OceansApp.Models.Models;
using OceansApp.Models.ViewModels.Components;
using OceansApp.Models.ViewModels.Projects;

namespace OceansApp.DataAccess.Repository.IRepository
{
    public interface IProjectRepository : IRepository<Project> 
    {
        Task<(List<ProjectsGetAllWithFiltersVM> projects, int totalCount)> GetAllProjectsWithFiltersAsync(ProjectsPaginationFiltersVM filtersAndPagination);
        Task<CreateUpdateProjectVM> GetProjectDataById(int projectId);
        Task<GetProjectConsultantAssignedVM> GetAssignedConsultantToProjectById(int consultantProjectAssignedtId);
        Task<MethodResponse> CreateProjectWithAssignedConsultants(CreateUpdateProjectVM projectData);
        Task<MethodResponse> UpdateProject(CreateUpdateProjectVM projectData);
        Task<MethodResponse> UpdateConsultantAssignedParameters(CreateUpdateProjectConsultantAssignedVM consultantAssignationData, string userUpdatedBy);
        void Update(Project obj);

    }
}
