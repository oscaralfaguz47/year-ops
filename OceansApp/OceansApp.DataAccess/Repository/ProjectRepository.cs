using Dapper;
using Microsoft.EntityFrameworkCore;
using OceansApp.DataAccess.Data;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.Models;
using OceansApp.Models.ViewModels.Components;
using OceansApp.Models.ViewModels.Projects;
using System.Data;

namespace OceansApp.DataAccess.Repository
{
    public class ProjectRepository : Repository<Project>, IProjectRepository
    {
        private ApplicationDbContext _db;
        public ProjectRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public async Task<(List<ProjectsGetAllWithFiltersVM> projects, int totalCount)> GetAllProjectsWithFiltersAsync(ProjectsPaginationFiltersVM filtersAndPagination)
        {
            var connection = _db.Database.GetDbConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@SearchText", filtersAndPagination.Filters.SearchText, DbType.String);
            parameters.Add("@StartDate", filtersAndPagination.Filters.StartDate, DbType.Date);
            parameters.Add("@EndDate", filtersAndPagination.Filters.EndDate, DbType.Date);
            parameters.Add("@IsActive", filtersAndPagination.Filters.IsActive, DbType.Boolean);
            parameters.Add("@ClientId", filtersAndPagination.Filters.ClientId, DbType.Int32);
            parameters.Add("@SuccessManagerId", filtersAndPagination.Filters.SuccessManagerId, DbType.Int32);
            parameters.Add("@FieldToOrder", filtersAndPagination.PaginationWithoutFilters.OrderBy.FieldToOrder, DbType.String);
            parameters.Add("@DirectionOrder", filtersAndPagination.PaginationWithoutFilters.OrderBy.DirectionOrder, DbType.String);
            parameters.Add("@Skip", (filtersAndPagination.PaginationWithoutFilters.Pagination.PageIndex - 1) * filtersAndPagination.PaginationWithoutFilters.Pagination.PageSize, DbType.Int32);
            parameters.Add("@Take", filtersAndPagination.PaginationWithoutFilters.Pagination.PageSize, DbType.Int32);
            parameters.Add("@TotalCount", dbType: DbType.Int32, direction: ParameterDirection.Output);

            var results = await connection.QueryAsync<ProjectsGetAllWithFiltersVM>("SP_GetAllProjectsWithFilters", parameters, commandType: CommandType.StoredProcedure);
            var totalCount = parameters.Get<int>("@TotalCount");

            var projects = results.ToList();

            return (projects, totalCount);
        }

        public async Task<MethodResponse> CreateProjectWithAssignedConsultants(CreateUpdateProjectVM projectData)
        {
            try
            {
                var costaRicaTime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, "Central America Standard Time");
                using var transaction = await _db.Database.BeginTransactionAsync();

                Project projectToCreate = new()
                {
                    Name = projectData.Name.Trim(),
                    Description = projectData.Description.Trim(),
                    StartDate = DateTime.Parse(projectData.StartDate),
                    IsActive = (bool)projectData.IsActive,
                    CreatedBy = projectData.CreatedBy,
                    CreationDate = costaRicaTime,
                    ClientId = (int)projectData.ClientId,
                    SuccessManagerId = (int)projectData.SuccessManagerId,
                    ClientHasTrackingTool = (bool)projectData.ClientHasTrackingTool
                };
                var createdProject = await _db.PROJECTS.AddAsync(projectToCreate);
                await _db.SaveChangesAsync();

                if (createdProject.Entity != null && createdProject.Entity.ProjectId > 0)
                {
                    if (projectData.AssignedConsultants != null)
                    {
                        foreach (var consultant in projectData.AssignedConsultants)
                        {
                            ProjectConsultantAssigned consultantAssignedToCreate = new()
                            {
                                ProjectId = createdProject.Entity.ProjectId,
                                ConsultantId = consultant.ConsultantId,
                                AssignedDate = costaRicaTime,
                                IsActive = true,
                                HourlyClientRate = consultant.HourlyClientRate,
                                HourlySalary = consultant.HourlySalary,
                                MonthlyClientRate = consultant.MonthlyClientRate,
                                MonthlySalary = consultant.MonthlySalary,
                                PositionDetail = consultant.PositionDetail
                            };
                          var createdAssignedConsultant =  await _db.PROJECTS_CONSULTANTS_ASSIGNED.AddAsync(consultantAssignedToCreate);
                            await _db.SaveChangesAsync();
                            if (createdAssignedConsultant.Entity != null && createdAssignedConsultant.Entity.ProjectConsultantAssignedId > 0)
                            {
                                ProjectConsultantAssignedHistory history = new()
                                {
                                    ProjectConsultantAssignedId = createdAssignedConsultant.Entity.ProjectConsultantAssignedId,
                                    Action = "Consultant Assigned First Time",
                                    ActionDate = costaRicaTime,
                                    UserActionedBy = projectData.CreatedBy,
                                    NewValue = $"The Consultant is assigned for the first time.",
                                };
                               await _db.PROJECTS_CONSULTANTS_ASSIGNED_HISTORY.AddAsync(history);
                               await _db.SaveChangesAsync();
                            }
                        }
                    }
                }
                else
                {
                    return new MethodResponse { MessageType = "Saving Error", Success = false, Message = $"The Project could not be created. Something went wrong. Please report this issue." };
                }
                await transaction.CommitAsync();
                return new MethodResponse { Success = true, Message = $"The Project was created successfully." };
            }
            catch (Exception ex)
            {
                return new MethodResponse { MessageType = "Exception Error", Success = false, Message = ex.Message };
            }
        }
        public void Update(Project obj)
        {
            _db.PROJECTS.Update(obj);
        }

    }
}
