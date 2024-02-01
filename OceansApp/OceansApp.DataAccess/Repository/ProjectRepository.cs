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

            var results = await connection.QueryAsync<ProjectsGetAllWithFiltersVM>("SP_PROJECTS_GetAllProjectsWithFilters", parameters, commandType: CommandType.StoredProcedure);
            var totalCount = parameters.Get<int>("@TotalCount");
            var projects = results.ToList();

            return (projects, totalCount);
        }

        public async Task<CreateUpdateProjectVM> GetProjectDataById(int projectId)
        {
            var connection = _db.Database.GetDbConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@ProjectId", projectId);

            using (var multiResultSet = await connection.QueryMultipleAsync("SP_PROJECTS_GetProjectDataById", parameters, commandType: CommandType.StoredProcedure))
            {
                var project = await multiResultSet.ReadFirstOrDefaultAsync<CreateUpdateProjectVM>();
                var assignedConsultants = await multiResultSet.ReadAsync<CreateUpdateProjectConsultantAssignedVM>();

                return new CreateUpdateProjectVM
                {
                    ProjectId = project.ProjectId,
                    Name = project.Name,
                    Description = project.Description,
                    StartDate = project.StartDate,
                    IsActive = project.IsActive,
                    IsBillable = project.IsBillable,
                    ClientId = project.ClientId,
                    ClientName = project.ClientName,
                    SuccessManagerId = project.SuccessManagerId,
                    SuccessManagerName = project.SuccessManagerName,
                    ClientHasTrackingTool = project.ClientHasTrackingTool,
                    AssignedConsultants = (List<CreateUpdateProjectConsultantAssignedVM>)assignedConsultants
                };
            }
        }

        public async Task<GetProjectConsultantAssignedVM> GetAssignedConsultantToProjectById(int consultantProjectAssignedtId)
        {
            var connection = _db.Database.GetDbConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@ProjectConsultantAssignedId", consultantProjectAssignedtId);

            var consultantAssignation = await connection.QuerySingleOrDefaultAsync<GetProjectConsultantAssignedVM>("SP_PROJECT_GetAssignedConsultantToProjectById",
                parameters, commandType: CommandType.StoredProcedure);
            return consultantAssignation;
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
                    IsBillable = (bool)projectData.IsBillable,
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
                            var createdAssignedConsultant = await _db.PROJECTS_CONSULTANTS_ASSIGNED.AddAsync(consultantAssignedToCreate);
                            await _db.SaveChangesAsync();
                            if (createdAssignedConsultant.Entity != null && createdAssignedConsultant.Entity.ProjectConsultantAssignedId > 0)
                            {
                                var clientRate = consultant.HourlyClientRate > 0 ? consultant.HourlyClientRate : consultant.MonthlyClientRate;
                                var consultantRate = consultant.HourlySalary > 0 ? consultant.HourlySalary : consultant.MonthlySalary;
                                ProjectConsultantAssignedHistory history = new()
                                {
                                    ProjectConsultantAssignedId = createdAssignedConsultant.Entity.ProjectConsultantAssignedId,
                                    Action = "Consultant Assigned First Time",
                                    ActionDate = costaRicaTime,
                                    UserActionedBy = projectData.CreatedBy,
                                    NewValue = $"The Consultant is assigned to the project with a rate of ${consultantRate} and with a rate for the client of ${clientRate}."
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

        public async Task<MethodResponse> UpdateProject(CreateUpdateProjectVM projectData)
        {
            try
            {
                var existingProject = await _db.PROJECTS.FirstOrDefaultAsync(x => x.ProjectId == projectData.ProjectId);
                var costaRicaTime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, "Central America Standard Time");
                using var transaction = await _db.Database.BeginTransactionAsync();

                existingProject.Name = projectData.Name;
                existingProject.Description = projectData.Description;
                existingProject.StartDate = DateTime.Parse(projectData.StartDate);
                existingProject.IsActive = (bool)projectData.IsActive;
                existingProject.SuccessManagerId = (int)projectData.SuccessManagerId;
                existingProject.ClientHasTrackingTool = (bool)projectData.ClientHasTrackingTool;
                existingProject.IsBillable = (bool)projectData.IsBillable;
                existingProject.UpdatedBy = projectData.CreatedBy;
                existingProject.DateLastUpdate = costaRicaTime;

                if (projectData.AssignedConsultants != null)
                {
                    foreach (var consultant in projectData.AssignedConsultants)
                    {
                        if (consultant.ProjectConsultantAssignedId == null)
                        {
                            ProjectConsultantAssigned consultantAssignedToCreate = new()
                            {
                                ProjectId = existingProject.ProjectId,
                                ConsultantId = consultant.ConsultantId,
                                AssignedDate = costaRicaTime,
                                IsActive = true,
                                HourlyClientRate = consultant.HourlyClientRate,
                                HourlySalary = consultant.HourlySalary,
                                MonthlyClientRate = consultant.MonthlyClientRate,
                                MonthlySalary = consultant.MonthlySalary,
                                PositionDetail = consultant.PositionDetail
                            };
                            var createdAssignedConsultant = await _db.PROJECTS_CONSULTANTS_ASSIGNED.AddAsync(consultantAssignedToCreate);
                            await _db.SaveChangesAsync();
                            if (createdAssignedConsultant.Entity != null && createdAssignedConsultant.Entity.ProjectConsultantAssignedId > 0)
                            {
                                var clientRate = consultant.HourlyClientRate > 0 ? consultant.HourlyClientRate : consultant.MonthlyClientRate;
                                var consultantRate = consultant.HourlySalary > 0 ? consultant.HourlySalary : consultant.MonthlySalary;
                                ProjectConsultantAssignedHistory history = new()
                                {
                                    ProjectConsultantAssignedId = createdAssignedConsultant.Entity.ProjectConsultantAssignedId,
                                    Action = "Consultant Assigned First Time",
                                    ActionDate = costaRicaTime,
                                    UserActionedBy = projectData.CreatedBy,
                                    NewValue = $"The Consultant is assigned to the project with a rate of ${consultantRate} and with a rate for the client of ${clientRate}."
                                };
                                await _db.PROJECTS_CONSULTANTS_ASSIGNED_HISTORY.AddAsync(history);
                                await _db.SaveChangesAsync();
                            }
                        }
                    }
                }

                await _db.SaveChangesAsync();
                await transaction.CommitAsync();
                return new MethodResponse { Success = true, Message = $"The Project was updated successfully." };
            }
            catch (Exception ex)
            {
                return new MethodResponse { MessageType = "Exception Error", Success = false, Message = ex.Message };
            }
        }

        public async Task<MethodResponse> UpdateConsultantAssignedParameters(CreateUpdateProjectConsultantAssignedVM consultantAssignationData, string userUpdatedBy)
        {
            try
            {
                var existingConsultantAssignation = await _db.PROJECTS_CONSULTANTS_ASSIGNED.FirstOrDefaultAsync(x => x.ProjectConsultantAssignedId ==
                consultantAssignationData.ProjectConsultantAssignedId);

                if (existingConsultantAssignation == null)
                {
                    return new MethodResponse { MessageType = "Validation Error", Success = false, Message = "The Consultant Assignation was not found." };
                }

                var costaRicaTime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, "Central America Standard Time");

                var historyToSaveList = new List<ProjectConsultantAssignedHistory>();

                if (existingConsultantAssignation.PositionDetail != consultantAssignationData.PositionDetail)
                {
                    var newHistory = new ProjectConsultantAssignedHistory();
                    newHistory.ProjectConsultantAssignedId = existingConsultantAssignation.ProjectConsultantAssignedId;
                    newHistory.ActionDate = costaRicaTime;
                    newHistory.UserActionedBy = userUpdatedBy;
                    newHistory.Action = "Position Details updated";
                    newHistory.NewValue = consultantAssignationData.PositionDetail;
                    newHistory.OldValue = existingConsultantAssignation.PositionDetail;
                    historyToSaveList.Add(newHistory);
                }
                if (existingConsultantAssignation.HourlyClientRate != consultantAssignationData.HourlyClientRate)
                {
                    var newHistory = new ProjectConsultantAssignedHistory();
                    newHistory.ProjectConsultantAssignedId = existingConsultantAssignation.ProjectConsultantAssignedId;
                    newHistory.ActionDate = costaRicaTime;
                    newHistory.UserActionedBy = userUpdatedBy;
                    newHistory.Action = "Hourly Client Rate updated";
                    newHistory.NewValue = consultantAssignationData.HourlyClientRate.ToString();
                    newHistory.OldValue = existingConsultantAssignation.HourlyClientRate.ToString();
                    historyToSaveList.Add(newHistory);
                }
                if (existingConsultantAssignation.MonthlyClientRate != consultantAssignationData.MonthlyClientRate)
                {
                    var newHistory = new ProjectConsultantAssignedHistory();
                    newHistory.ProjectConsultantAssignedId = existingConsultantAssignation.ProjectConsultantAssignedId;
                    newHistory.ActionDate = costaRicaTime;
                    newHistory.UserActionedBy = userUpdatedBy;
                    newHistory.Action = "Monthly Client Rate updated";
                    newHistory.NewValue = consultantAssignationData.MonthlyClientRate.ToString();
                    newHistory.OldValue = existingConsultantAssignation.MonthlyClientRate.ToString();
                    historyToSaveList.Add(newHistory);
                }
                if (existingConsultantAssignation.HourlySalary != consultantAssignationData.HourlySalary)
                {
                    var newHistory = new ProjectConsultantAssignedHistory();
                    newHistory.ProjectConsultantAssignedId = existingConsultantAssignation.ProjectConsultantAssignedId;
                    newHistory.ActionDate = costaRicaTime;
                    newHistory.UserActionedBy = userUpdatedBy;
                    newHistory.Action = "Hourly Salary updated";
                    newHistory.NewValue = consultantAssignationData.HourlySalary.ToString();
                    newHistory.OldValue = existingConsultantAssignation.HourlySalary.ToString();
                    historyToSaveList.Add(newHistory);
                }
                if (existingConsultantAssignation.MonthlySalary != consultantAssignationData.MonthlySalary)
                {
                    var newHistory = new ProjectConsultantAssignedHistory();
                    newHistory.ProjectConsultantAssignedId = existingConsultantAssignation.ProjectConsultantAssignedId;
                    newHistory.ActionDate = costaRicaTime;
                    newHistory.UserActionedBy = userUpdatedBy;
                    newHistory.Action = "Monthly Salary updated";
                    newHistory.NewValue = consultantAssignationData.MonthlySalary.ToString();
                    newHistory.OldValue = existingConsultantAssignation.MonthlySalary.ToString();
                    historyToSaveList.Add(newHistory);
                }
                if (existingConsultantAssignation.HourlyClientRate > 0 && consultantAssignationData.HourlyClientRate == 0)
                {
                    var newHistory = new ProjectConsultantAssignedHistory();
                    newHistory.ProjectConsultantAssignedId = existingConsultantAssignation.ProjectConsultantAssignedId;
                    newHistory.ActionDate = costaRicaTime;
                    newHistory.UserActionedBy = userUpdatedBy;
                    newHistory.Action = "Client pricing method updated";
                    newHistory.NewValue = "Monthly";
                    newHistory.OldValue = "Hourly";
                    historyToSaveList.Add(newHistory);
                }
                if (existingConsultantAssignation.MonthlyClientRate > 0 && consultantAssignationData.MonthlyClientRate == 0)
                {
                    var newHistory = new ProjectConsultantAssignedHistory();
                    newHistory.ProjectConsultantAssignedId = existingConsultantAssignation.ProjectConsultantAssignedId;
                    newHistory.ActionDate = costaRicaTime;
                    newHistory.UserActionedBy = userUpdatedBy;
                    newHistory.Action = "Client pricing method updated";
                    newHistory.NewValue = "Hourly";
                    newHistory.OldValue = "Monthly";
                    historyToSaveList.Add(newHistory);
                }
                if (existingConsultantAssignation.HourlySalary > 0 && consultantAssignationData.HourlySalary == 0)
                {
                    var newHistory = new ProjectConsultantAssignedHistory();
                    newHistory.ProjectConsultantAssignedId = existingConsultantAssignation.ProjectConsultantAssignedId;
                    newHistory.ActionDate = costaRicaTime;
                    newHistory.UserActionedBy = userUpdatedBy;
                    newHistory.Action = "Consultant pricing method updated";
                    newHistory.NewValue = "Monthly";
                    newHistory.OldValue = "Hourly";
                    historyToSaveList.Add(newHistory);
                }
                if (existingConsultantAssignation.MonthlySalary > 0 && consultantAssignationData.MonthlySalary == 0)
                {
                    var newHistory = new ProjectConsultantAssignedHistory();
                    newHistory.ProjectConsultantAssignedId = existingConsultantAssignation.ProjectConsultantAssignedId;
                    newHistory.ActionDate = costaRicaTime;
                    newHistory.UserActionedBy = userUpdatedBy;
                    newHistory.Action = "Consultant pricing method updated";
                    newHistory.NewValue = "Hourly";
                    newHistory.OldValue = "Monthly";
                    historyToSaveList.Add(newHistory);
                }
                using var transaction = await _db.Database.BeginTransactionAsync();

                existingConsultantAssignation.PositionDetail = consultantAssignationData.PositionDetail;
                existingConsultantAssignation.MonthlyClientRate = consultantAssignationData.MonthlyClientRate;
                existingConsultantAssignation.HourlyClientRate = consultantAssignationData.HourlyClientRate;
                existingConsultantAssignation.MonthlySalary = consultantAssignationData.MonthlySalary;
                existingConsultantAssignation.HourlySalary = consultantAssignationData.HourlySalary;

                await _db.SaveChangesAsync();

                foreach (var historyToAdd in historyToSaveList)
                {
                    await _db.PROJECTS_CONSULTANTS_ASSIGNED_HISTORY.AddAsync(historyToAdd);
                }
                await _db.SaveChangesAsync();
                await transaction.CommitAsync();
                return new MethodResponse { Success = true, Message = $"The Consultant Parameters were updated successfully." };
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
