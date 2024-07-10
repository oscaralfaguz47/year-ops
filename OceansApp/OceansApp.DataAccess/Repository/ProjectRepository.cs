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

        public async Task<MethodResponse> CreateProject(CreateUpdateProjectVM projectData)
        {
            try
            {
                var existingproject = await _db.PROJECTS.FirstOrDefaultAsync(x => x.Name.Trim() == projectData.Name.Trim());
                if (existingproject != null)
                {
                    return new MethodResponse { MessageType = "Validation Error", Success = false, Message = $"There is already a project with the name: {projectData.Name}" };
                }
                var userActionedBy = await _db.CONSULTANT_DETAILS.FirstOrDefaultAsync(x => x.UserId == projectData.CreatedBy);
                using var transaction = await _db.Database.BeginTransactionAsync();

                Project projectToCreate = new()
                {
                    Name = projectData.Name.Trim(),
                    Description = projectData.Description.Trim(),
                    StartDate = DateTime.Parse(projectData.StartDate),
                    IsActive = (bool)projectData.IsActive,
                    IsBillable = (bool)projectData.IsBillable,
                    CreatedBy = projectData.CreatedBy,
                    CreationDate = DateTime.UtcNow,
                    ClientId = (int)projectData.ClientId,
                    SuccessManagerId = (int)projectData.SuccessManagerId,
                    ClientHasTrackingTool = (bool)projectData.ClientHasTrackingTool
                };
                var createdProject = await _db.PROJECTS.AddAsync(projectToCreate);
                await _db.SaveChangesAsync();
                await transaction.CommitAsync();
                if (createdProject.Entity.ProjectId > 0)
                {
                    return new MethodResponse
                    {
                        Success = true,
                        Message = $"The Project {projectData.Name} was created successfully.",
                        IdCreatedElement = createdProject.Entity.ProjectId
                    };
                }
                else
                {
                    return new MethodResponse { MessageType = "Exception Error", Success = false, Message = "Something went wrong creating the project, please try again." };
                }

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
                var duplicatedProject = await _db.PROJECTS.FirstOrDefaultAsync(x => x.Name.Trim() == projectData.Name.Trim());
                var existingProject = await _db.PROJECTS.FirstOrDefaultAsync(x => x.ProjectId == projectData.ProjectId);

                if (duplicatedProject != null && existingProject.ProjectId != duplicatedProject.ProjectId)
                {
                    return new MethodResponse { MessageType = "Validation Error", Success = false, Message = $"There is already a project with the name: {projectData.Name}" };
                }

                var userActionedBy = await _db.CONSULTANT_DETAILS.FirstOrDefaultAsync(x => x.UserId == projectData.CreatedBy);
                using var transaction = await _db.Database.BeginTransactionAsync();

                existingProject.Name = projectData.Name;
                existingProject.Description = projectData.Description;
                existingProject.StartDate = DateTime.Parse(projectData.StartDate);
                existingProject.IsActive = (bool)projectData.IsActive;
                existingProject.SuccessManagerId = (int)projectData.SuccessManagerId;
                existingProject.UpdatedBy = projectData.CreatedBy;
                existingProject.DateLastUpdate = DateTime.UtcNow;

                if (projectData.AssignedConsultants != null)
                {
                    foreach (var consultant in projectData.AssignedConsultants)
                    {
                        if (consultant.ProjectConsultantAssignedId == null)
                        {
                            var projectAssignations = await _db.PROJECTS_CONSULTANTS_ASSIGNED.Where(x => x.ConsultantId == consultant.ConsultantId).ToListAsync();
                            var defaultProject = false;
                            if (projectAssignations.Count == 0 || consultant.IsDefaultProject)
                            {
                                defaultProject = true;
                            }

                            if (projectAssignations.Count > 0 && consultant.IsDefaultProject)
                            {
                                foreach (var projectAss in projectAssignations)
                                {
                                    projectAss.IsDefaultProject = false;
                                }
                            }
                            ProjectConsultantAssigned consultantAssignedToCreate = new()
                            {
                                ProjectId = existingProject.ProjectId,
                                ConsultantId = consultant.ConsultantId,
                                CreationDate = DateTime.UtcNow,
                                IsActive = true,
                                HourlyClientRate = consultant.HourlyClientRate,
                                HourlySalary = consultant.HourlySalary,
                                MonthlyClientRate = consultant.MonthlyClientRate,
                                MonthlySalary = consultant.MonthlySalary,
                                MonthlySalaryThirdParty = consultant.MonthlySalaryThirdParty,
                                PartnerId = consultant.PartnerId,
                                PositionId = consultant.PositionId,
                                IsMonthlySalaryCalculatedPerHour = consultant.IsMonthlySalaryCalculatedPerHour,
                                AccessToTrackingTool = consultant.AccessToTrackingTool,
                                IsDefaultProject = defaultProject
                            };
                            var createdAssignedConsultant = await _db.PROJECTS_CONSULTANTS_ASSIGNED.AddAsync(consultantAssignedToCreate);
                            await _db.SaveChangesAsync();
                            if (createdAssignedConsultant.Entity != null && createdAssignedConsultant.Entity.ProjectConsultantAssignedId > 0)
                            {
                                var consultantToAssign = await _db.CONSULTANT_DETAILS.FirstOrDefaultAsync(x => x.ConsultantId == consultant.ConsultantId);
                                if (consultantToAssign == null)
                                {
                                    return new MethodResponse { MessageType = "Exception Error", Success = false, Message = "The consultant was not found." };
                                }
                                bool consultantAssignedProjects = await _db.PROJECTS_USERS_SELECTED.AnyAsync(x => x.UserId == consultantToAssign.UserId);
                                if (!consultantAssignedProjects)
                                {
                                    ProjectUserSelected projectSelectedToCreate = new()
                                    {
                                        ProjectId = existingProject.ProjectId,
                                        UserId = consultantToAssign.UserId
                                    };
                                    await _db.PROJECTS_USERS_SELECTED.AddAsync(projectSelectedToCreate);
                                }
                                var clientRate = consultant.HourlyClientRate > 0 ? consultant.HourlyClientRate : consultant.MonthlyClientRate;
                                var consultantRate = consultant.HourlySalary > 0 ? consultant.HourlySalary : consultant.MonthlySalary;
                                var clientRateMethod = consultant.MonthlyClientRate > 0 ? "Monthly" : "Hourly";
                                var consultantRateMethod = consultant.MonthlySalary > 0 ? "Monthly" : "Hourly";
                                var action = await _db.PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_ACTIONS.FirstOrDefaultAsync(x => x.Name == "Consultant Assigned First Time");
                                ProjectConsultantAssignedHistory historyClient = new()
                                {
                                    ProjectConsultantAssignedId = createdAssignedConsultant.Entity.ProjectConsultantAssignedId,
                                    ActionId = action.ActionId,
                                    ActionDate = DateTime.Parse(consultant.ActionDate),
                                    CreationDate = DateTime.UtcNow,
                                    UserActionedBy = userActionedBy.ConsultantId,
                                    NewValue = clientRate,
                                    NewValueDetail = clientRateMethod
                                };
                                await _db.PROJECTS_CONSULTANTS_ASSIGNED_HISTORY.AddAsync(historyClient);

                                ProjectConsultantAssignedHistory historyConsultant = new()
                                {
                                    ProjectConsultantAssignedId = createdAssignedConsultant.Entity.ProjectConsultantAssignedId,
                                    ActionId = action.ActionId,
                                    ActionDate = DateTime.Parse(consultant.ActionDate),
                                    CreationDate = DateTime.UtcNow,
                                    UserActionedBy = userActionedBy.ConsultantId,
                                    NewValue = consultantRate,
                                    NewValueDetail = consultantRateMethod
                                };
                                await _db.PROJECTS_CONSULTANTS_ASSIGNED_HISTORY.AddAsync(historyConsultant);
                                ProjectConsultantAssignedHistory historyDetail = new()
                                {
                                    ProjectConsultantAssignedId = createdAssignedConsultant.Entity.ProjectConsultantAssignedId,
                                    ActionId = action.ActionId,
                                    ActionDate = DateTime.Parse(consultant.ActionDate),
                                    CreationDate = DateTime.UtcNow,
                                    UserActionedBy = userActionedBy.ConsultantId
                                };
                                await _db.PROJECTS_CONSULTANTS_ASSIGNED_HISTORY.AddAsync(historyDetail);
                                if (consultant.MonthlySalaryThirdParty > 0)
                                {
                                    ProjectConsultantAssignedHistory historyThirdParty = new()
                                    {
                                        ProjectConsultantAssignedId = createdAssignedConsultant.Entity.ProjectConsultantAssignedId,
                                        ActionId = action.ActionId,
                                        ActionDate = DateTime.Parse(consultant.ActionDate),
                                        CreationDate = DateTime.UtcNow,
                                        UserActionedBy = userActionedBy.ConsultantId,
                                        NewValue = consultant.MonthlySalaryThirdParty,
                                        NewValueDetail = "Consultant Third Party Mothly Salary"
                                    };
                                    await _db.PROJECTS_CONSULTANTS_ASSIGNED_HISTORY.AddAsync(historyThirdParty);
                                }


                                await _db.SaveChangesAsync();
                            }
                        }
                    }
                }

                await _db.SaveChangesAsync();
                await transaction.CommitAsync();
                return new MethodResponse { Success = true, Message = $"The Project {existingProject.Name} was updated successfully." };
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

                var userActionedBy = await _db.CONSULTANT_DETAILS.FirstOrDefaultAsync(x => x.UserId == userUpdatedBy);

                var historyToSaveList = new List<ProjectConsultantAssignedHistory>();

                if (existingConsultantAssignation.HourlyClientRate != consultantAssignationData.HourlyClientRate)
                {
                    var action = await _db.PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_ACTIONS.FirstOrDefaultAsync(x => x.Name == "Hourly Client Rate updated");
                    var newHistory = new ProjectConsultantAssignedHistory();
                    newHistory.ProjectConsultantAssignedId = existingConsultantAssignation.ProjectConsultantAssignedId;
                    newHistory.ActionDate = DateTime.Parse(consultantAssignationData.ActionDate);
                    newHistory.CreationDate = DateTime.UtcNow;
                    newHistory.UserActionedBy = userActionedBy.ConsultantId;
                    newHistory.ActionId = action.ActionId;
                    newHistory.NewValue = consultantAssignationData.HourlyClientRate;
                    newHistory.OldValue = existingConsultantAssignation.HourlyClientRate;
                    historyToSaveList.Add(newHistory);
                }
                if (existingConsultantAssignation.MonthlyClientRate != consultantAssignationData.MonthlyClientRate)
                {
                    var action = await _db.PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_ACTIONS.FirstOrDefaultAsync(x => x.Name == "Monthly Client Rate updated");
                    var newHistory = new ProjectConsultantAssignedHistory();
                    newHistory.ProjectConsultantAssignedId = existingConsultantAssignation.ProjectConsultantAssignedId;
                    newHistory.ActionDate = DateTime.Parse(consultantAssignationData.ActionDate);
                    newHistory.CreationDate = DateTime.UtcNow;
                    newHistory.UserActionedBy = userActionedBy.ConsultantId;
                    newHistory.ActionId = action.ActionId;
                    newHistory.NewValue = consultantAssignationData.MonthlyClientRate;
                    newHistory.OldValue = existingConsultantAssignation.MonthlyClientRate;
                    historyToSaveList.Add(newHistory);
                }
                if (existingConsultantAssignation.HourlySalary != consultantAssignationData.HourlySalary)
                {
                    var action = await _db.PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_ACTIONS.FirstOrDefaultAsync(x => x.Name == "Hourly Salary updated");
                    var newHistory = new ProjectConsultantAssignedHistory();
                    newHistory.ProjectConsultantAssignedId = existingConsultantAssignation.ProjectConsultantAssignedId;
                    newHistory.ActionDate = DateTime.Parse(consultantAssignationData.ActionDate);
                    newHistory.CreationDate = DateTime.UtcNow;
                    newHistory.UserActionedBy = userActionedBy.ConsultantId;
                    newHistory.ActionId = action.ActionId;
                    newHistory.NewValue = consultantAssignationData.HourlySalary;
                    newHistory.OldValue = existingConsultantAssignation.HourlySalary;
                    historyToSaveList.Add(newHistory);
                }
                if (existingConsultantAssignation.MonthlySalary != consultantAssignationData.MonthlySalary)
                {
                    var action = await _db.PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_ACTIONS.FirstOrDefaultAsync(x => x.Name == "Monthly Salary updated");
                    var newHistory = new ProjectConsultantAssignedHistory();
                    newHistory.ProjectConsultantAssignedId = existingConsultantAssignation.ProjectConsultantAssignedId;
                    newHistory.ActionDate = DateTime.Parse(consultantAssignationData.ActionDate);
                    newHistory.CreationDate = DateTime.UtcNow;
                    newHistory.UserActionedBy = userActionedBy.ConsultantId;
                    newHistory.ActionId = action.ActionId;
                    newHistory.NewValue = consultantAssignationData.MonthlySalary;
                    newHistory.OldValue = existingConsultantAssignation.MonthlySalary;
                    historyToSaveList.Add(newHistory);
                }
                if (existingConsultantAssignation.MonthlySalaryThirdParty != consultantAssignationData.MonthlySalaryThirdParty)
                {
                    var action = await _db.PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_ACTIONS.FirstOrDefaultAsync(x => x.Name == "Third Party Salary updated");
                    var newHistory = new ProjectConsultantAssignedHistory();
                    newHistory.ProjectConsultantAssignedId = existingConsultantAssignation.ProjectConsultantAssignedId;
                    newHistory.ActionDate = DateTime.Parse(consultantAssignationData.ActionDate);
                    newHistory.CreationDate = DateTime.UtcNow;
                    newHistory.UserActionedBy = userActionedBy.ConsultantId;
                    newHistory.ActionId = action.ActionId;
                    newHistory.NewValue = consultantAssignationData.MonthlySalaryThirdParty;
                    newHistory.OldValue = existingConsultantAssignation.MonthlySalaryThirdParty;
                    historyToSaveList.Add(newHistory);
                }
                if (existingConsultantAssignation.HourlyClientRate > 0 && consultantAssignationData.HourlyClientRate == 0)
                {
                    var action = await _db.PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_ACTIONS.FirstOrDefaultAsync(x => x.Name == "Client pricing method updated (Monthly)");
                    var newHistory = new ProjectConsultantAssignedHistory();
                    newHistory.ProjectConsultantAssignedId = existingConsultantAssignation.ProjectConsultantAssignedId;
                    newHistory.ActionDate = DateTime.Parse(consultantAssignationData.ActionDate);
                    newHistory.CreationDate = DateTime.UtcNow;
                    newHistory.UserActionedBy = userActionedBy.ConsultantId;
                    newHistory.ActionId = action.ActionId;
                    historyToSaveList.Add(newHistory);
                }
                if (existingConsultantAssignation.MonthlyClientRate > 0 && consultantAssignationData.MonthlyClientRate == 0)
                {
                    var action = await _db.PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_ACTIONS.FirstOrDefaultAsync(x => x.Name == "Client pricing method updated (Hourly)");
                    var newHistory = new ProjectConsultantAssignedHistory();
                    newHistory.ProjectConsultantAssignedId = existingConsultantAssignation.ProjectConsultantAssignedId;
                    newHistory.ActionDate = DateTime.Parse(consultantAssignationData.ActionDate);
                    newHistory.CreationDate = DateTime.UtcNow;
                    newHistory.UserActionedBy = userActionedBy.ConsultantId;
                    newHistory.ActionId = action.ActionId;
                    historyToSaveList.Add(newHistory);
                }
                if ((existingConsultantAssignation.HourlySalary > 0 || existingConsultantAssignation.MonthlySalaryThirdParty > 0) && consultantAssignationData.HourlySalary == 0 
                    && consultantAssignationData.MonthlySalary > 0)
                {
                    var action = await _db.PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_ACTIONS.FirstOrDefaultAsync(x => x.Name == "Consultant pricing method updated (Monthly)");
                    var newHistory = new ProjectConsultantAssignedHistory();
                    newHistory.ProjectConsultantAssignedId = existingConsultantAssignation.ProjectConsultantAssignedId;
                    newHistory.ActionDate = DateTime.Parse(consultantAssignationData.ActionDate);
                    newHistory.CreationDate = DateTime.UtcNow;
                    newHistory.UserActionedBy = userActionedBy.ConsultantId;
                    newHistory.ActionId = action.ActionId;
                    historyToSaveList.Add(newHistory);
                }
                if ((existingConsultantAssignation.MonthlySalary > 0 || existingConsultantAssignation.MonthlySalaryThirdParty > 0) && consultantAssignationData.MonthlySalary == 0 
                    && consultantAssignationData.HourlySalary > 0)
                {
                    var action = await _db.PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_ACTIONS.FirstOrDefaultAsync(x => x.Name == "Consultant pricing method updated (Hourly)");
                    var newHistory = new ProjectConsultantAssignedHistory();
                    newHistory.ProjectConsultantAssignedId = existingConsultantAssignation.ProjectConsultantAssignedId;
                    newHistory.ActionDate = DateTime.Parse(consultantAssignationData.ActionDate);
                    newHistory.CreationDate = DateTime.UtcNow;
                    newHistory.UserActionedBy = userActionedBy.ConsultantId;
                    newHistory.ActionId = action.ActionId;
                    historyToSaveList.Add(newHistory);
                }
                using var transaction = await _db.Database.BeginTransactionAsync();

                if (consultantAssignationData.IsDefaultProject)
                {
                    var projectAssignations = await _db.PROJECTS_CONSULTANTS_ASSIGNED.Where(x => x.ConsultantId == existingConsultantAssignation.ConsultantId).ToListAsync();
                    foreach (var projectAss in projectAssignations)
                    {
                        projectAss.IsDefaultProject = false;
                    }
                    existingConsultantAssignation.IsDefaultProject = true;
                }

                existingConsultantAssignation.PositionId = consultantAssignationData.PositionId;
                existingConsultantAssignation.MonthlyClientRate = consultantAssignationData.MonthlyClientRate;
                existingConsultantAssignation.HourlyClientRate = consultantAssignationData.HourlyClientRate;
                existingConsultantAssignation.MonthlySalary = consultantAssignationData.MonthlySalary;
                existingConsultantAssignation.HourlySalary = consultantAssignationData.HourlySalary;
                existingConsultantAssignation.MonthlySalaryThirdParty = consultantAssignationData.MonthlySalaryThirdParty;
                existingConsultantAssignation.PartnerId = consultantAssignationData.PartnerId;
                existingConsultantAssignation.IsMonthlySalaryCalculatedPerHour = consultantAssignationData.IsMonthlySalaryCalculatedPerHour;
                existingConsultantAssignation.AccessToTrackingTool = consultantAssignationData.AccessToTrackingTool;

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
