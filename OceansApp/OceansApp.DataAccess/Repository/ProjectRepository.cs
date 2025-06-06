using Dapper;
using Microsoft.EntityFrameworkCore;
using OceansApp.DataAccess.Data;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.Models;
using OceansApp.Models.ViewModels.Components;
using OceansApp.Models.ViewModels.Projects;
using System.Data;
using System.Reflection;
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

        public async Task<CreateUpdateProjectVM> GetProjectDataByIdAsync(int projectId)
        {
            var connection = _db.Database.GetDbConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@ProjectId", projectId);
            parameters.Add("@CurrentDate", DateTime.UtcNow);

            try
            {
                using (var multiResultSet = await connection.QueryMultipleAsync("SP_PROJECTS_GetProjectDataById", parameters, commandType: CommandType.StoredProcedure))
                {
                    return await multiResultSet.ReadFirstOrDefaultAsync<CreateUpdateProjectVM>();
                }
            }
            catch (Exception ex)
            {
                throw;
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

                await _db.SaveChangesAsync();
                await transaction.CommitAsync();
                return new MethodResponse { Success = true, Message = $"The Project {existingProject.Name} was updated successfully." };
            }
            catch (Exception ex)
            {
                return new MethodResponse { MessageType = "Exception Error", Success = false, Message = ex.Message };
            }
        }

        public async Task<List<GetDataForSelectVM>> GetAllProjectsWithActiveInactiveAsync()
        {
            var results = await _db.PROJECTS.OrderBy(x => x.Name).ToListAsync();
            List<GetDataForSelectVM> projectsList = new List<GetDataForSelectVM>();

            foreach (var project in results)
            {
                GetDataForSelectVM projectToAdd = new GetDataForSelectVM()
                {
                    Text = $"{project.Name} ({(project.IsActive ? "Active" : "Inactive")})",
                    Value = project.ProjectId
                };
                projectsList.Add(projectToAdd);
            }
            return projectsList;
        }

        public async Task<MethodResponse> AddUpdateConsultantInProjet(CreateUpdateProjectConsultantHistoryVM consultantAssignationData)
        {
            try
            {
                string messageSuccess = "";
                var existingProject = await _db.PROJECTS.FirstOrDefaultAsync(x => x.ProjectId == consultantAssignationData.ProjectId);
                if (existingProject == null)
                {
                    return MethodResponse.CreateFailureValidationResponse("The project you are trying to add the consultant to no longer exists.", "ProjectId");
                }
                if (existingProject.ClientHasTrackingTool)
                {
                    if (consultantAssignationData.PrimaryReportTrackingToolName == null)
                    {
                        return MethodResponse.CreateFailureValidationResponse("Our Client's tracking tool name is required.");
                    }
                    if (consultantAssignationData.PrimaryReportTrackingToolName.Length > 30)
                    {
                        return MethodResponse.CreateFailureValidationResponse("Our Client's tracking tool name must be between 1 and 30 characters.");
                    }
                    if ((bool)consultantAssignationData.NeedSecondReportTrackingTool)
                    {
                        if (consultantAssignationData.SecondReportTrackingToolName == null)
                        {
                            return MethodResponse.CreateFailureValidationResponse("The second tracking tool name is required.");
                        }
                        if (consultantAssignationData.SecondReportTrackingToolName.Length > 30)
                        {
                            return MethodResponse.CreateFailureValidationResponse("The second tracking tool name must be between 1 and 30 characters.");
                        }
                    }
                }
                var existingConsultant = await _db.CONSULTANT_DETAILS.FirstOrDefaultAsync(x => x.ConsultantId == consultantAssignationData.ConsultantId);
                if (existingConsultant == null)
                {
                    return MethodResponse.CreateFailureValidationResponse("The consultant you are trying to add to the project to no longer exists.", "ConsultantId");
                }

                var existingConsultantAssignation = await _db.PROJECTS_CONSULTANTS_ASSIGNED
                    .FirstOrDefaultAsync(x => x.ConsultantId == consultantAssignationData.ConsultantId &&
                    x.ProjectId == consultantAssignationData.ProjectId);

                if (existingConsultantAssignation != null && (bool)consultantAssignationData.IsAssigningFirstTime)
                {
                    return MethodResponse.CreateFailureValidationResponse("The consultant you are trying to add to this project is already added.", "IsAssigningFirstTime");
                }

                using var transaction = await _db.Database.BeginTransactionAsync();

                var projectAssignations = await _db.PROJECTS_CONSULTANTS_ASSIGNED
                    .Where(x => x.ConsultantId == consultantAssignationData.ConsultantId).ToListAsync();
                bool defaultProject = false;
                if (projectAssignations.Count == 0 || (bool)consultantAssignationData.IsDefaultProject)
                {
                    defaultProject = true;
                    if (projectAssignations.Count == 0)
                    {
                        var consultantUser = await _db.Users.FirstOrDefaultAsync(x => x.Id == existingConsultant.UserId);
                        ProjectUserSelected projectUserSelectedToCreate = new()
                        {
                            ProjectId = existingProject.ProjectId,
                            UserId = consultantUser.Id
                        };
                        await _db.PROJECTS_USERS_SELECTED.AddAsync(projectUserSelectedToCreate);
                        await _db.SaveChangesAsync();
                    }
                }

                if (projectAssignations.Count > 0 && (bool)consultantAssignationData.IsDefaultProject)
                {
                    if ((bool)consultantAssignationData.IsDefaultProject)
                    {
                        foreach (var conAssignation in projectAssignations)
                        {
                            if (conAssignation.ProjectId != consultantAssignationData.ProjectId)
                            {
                                var currentProjectAssignationHistory = await _db.PROJECTS_CONSULTANTS_ASSIGNED_HISTORY
                            .Where(x => x.ProjectConsultantAssignedId == conAssignation.ProjectConsultantAssignedId
                            && x.ActionDate <= consultantAssignationData.ActionDate).OrderByDescending(x => x.ActionDate)
                            .ThenByDescending(x => x.Id).FirstOrDefaultAsync();

                                if (currentProjectAssignationHistory == null)
                                {
                                    currentProjectAssignationHistory = await _db.PROJECTS_CONSULTANTS_ASSIGNED_HISTORY
                            .Where(x => x.ProjectConsultantAssignedId == conAssignation.ProjectConsultantAssignedId
                            && x.ActionDate >= consultantAssignationData.ActionDate).OrderBy(x => x.ActionDate)
                            .ThenByDescending(x => x.Id).FirstOrDefaultAsync();
                                }

                                if (currentProjectAssignationHistory.IsDefaultProject == consultantAssignationData.IsDefaultProject)
                                {

                                    ProjectConsultantAssignedHistory historyToCreateChangingIsDefaultProject = new();

                                    foreach (PropertyInfo property in typeof(ProjectConsultantAssignedHistory).GetProperties())
                                    {
                                        if (property.Name != "Id")
                                        {
                                            property.SetValue(historyToCreateChangingIsDefaultProject, property.GetValue(currentProjectAssignationHistory));
                                        }
                                    }

                                    historyToCreateChangingIsDefaultProject.IsDefaultProject = false;
                                    historyToCreateChangingIsDefaultProject.ActionDate = (DateTime)consultantAssignationData.ActionDate;
                                    historyToCreateChangingIsDefaultProject.CreationDate = DateTime.UtcNow;
                                    historyToCreateChangingIsDefaultProject.UserIdActionedBy = consultantAssignationData.UserCreatedBy;
                                    await _db.PROJECTS_CONSULTANTS_ASSIGNED_HISTORY.AddAsync(historyToCreateChangingIsDefaultProject);
                                    await _db.SaveChangesAsync();
                                }
                            }
                        }
                    }
                }

                ProjectConsultantAssignedHistory consultantAssignedHistoryToCreate = new()
                {
                    HourlyClientRate = consultantAssignationData.HourlyClientRate,
                    MonthlyClientRate = consultantAssignationData.MonthlyClientRate,
                    MonthlyClientRateNumDays = consultantAssignationData.MonthlyClientRateNumDays,
                    HourlySalary = consultantAssignationData.HourlySalary,
                    MonthlySalary = consultantAssignationData.MonthlySalary,
                    MonthlySalaryPartner = consultantAssignationData.MonthlySalaryPartner,
                    PartnerId = consultantAssignationData.PartnerId,
                    PartnerPaysBenefits = (bool)consultantAssignationData.PartnerPaysBenefits,
                    PositionId = (int)consultantAssignationData.PositionId,
                    IsMonthlySalaryCalculatedPerHour = consultantAssignationData.IsMonthlySalaryCalculatedPerHour,
                    AccessToTrackingTool = (bool)consultantAssignationData.AccessToTrackingTool,
                    IsDefaultProject = defaultProject,
                    HolidaysMustBePaid = (bool)consultantAssignationData.HolidaysMustBePaid,
                    ActionDate = (DateTime)consultantAssignationData.ActionDate,
                    CreationDate = DateTime.UtcNow,
                    UserIdActionedBy = consultantAssignationData.UserCreatedBy,
                    ParticipatesInOnCalls = (bool)consultantAssignationData.ParticipatesInOnCalls,
                    NumHoursForHoliday = consultantAssignationData.NumHoursForHoliday == null ? 8 : (int)consultantAssignationData.NumHoursForHoliday,
                    PrimaryReportTrackingToolName = consultantAssignationData.PrimaryReportTrackingToolName,
                    SecondReportTrackingToolName = consultantAssignationData.SecondReportTrackingToolName
                };

                if (existingConsultantAssignation == null && (bool)consultantAssignationData.IsAssigningFirstTime)
                {
                    ProjectConsultantAssigned consultantAssignationForCreateOrUpdate = new()
                    {
                        ProjectId = (int)consultantAssignationData.ProjectId,
                        ConsultantId = (int)consultantAssignationData.ConsultantId
                    };
                    await _db.PROJECTS_CONSULTANTS_ASSIGNED.AddAsync(consultantAssignationForCreateOrUpdate);
                    await _db.SaveChangesAsync();
                    consultantAssignedHistoryToCreate.ProjectConsultantAssignedId = consultantAssignationForCreateOrUpdate.ProjectConsultantAssignedId;
                    consultantAssignedHistoryToCreate.IsActive = true;

                    await _db.PROJECTS_CONSULTANTS_ASSIGNED_HISTORY.AddAsync(consultantAssignedHistoryToCreate);
                    await _db.SaveChangesAsync();
                    messageSuccess = "The Consultant was assigned to the project successfully!";
                }

                if (existingConsultantAssignation != null && !(bool)consultantAssignationData.IsAssigningFirstTime)
                {
                    var recentHistoryBeforeActionDate = await _db.PROJECTS_CONSULTANTS_ASSIGNED_HISTORY
                        .Where(x => x.ProjectConsultantAssignedId == existingConsultantAssignation.ProjectConsultantAssignedId &&
                                    x.ActionDate <= consultantAssignationData.ActionDate)
                        .OrderByDescending(x => x.ActionDate).ThenByDescending(x => x.Id)
                        .FirstOrDefaultAsync();

                    consultantAssignedHistoryToCreate.ProjectConsultantAssignedId = existingConsultantAssignation.ProjectConsultantAssignedId;
                    consultantAssignedHistoryToCreate.IsActive = recentHistoryBeforeActionDate.IsActive;

                    if (
                        consultantAssignedHistoryToCreate.PositionId != recentHistoryBeforeActionDate.PositionId ||
                        consultantAssignedHistoryToCreate.HourlySalary != recentHistoryBeforeActionDate.HourlySalary ||
                        consultantAssignedHistoryToCreate.MonthlySalary != recentHistoryBeforeActionDate.MonthlySalary ||
                        consultantAssignedHistoryToCreate.IsMonthlySalaryCalculatedPerHour != recentHistoryBeforeActionDate.IsMonthlySalaryCalculatedPerHour ||
                        consultantAssignedHistoryToCreate.MonthlySalaryPartner != recentHistoryBeforeActionDate.MonthlySalaryPartner ||
                        consultantAssignedHistoryToCreate.PartnerId != recentHistoryBeforeActionDate.PartnerId ||
                        consultantAssignedHistoryToCreate.PartnerPaysBenefits != recentHistoryBeforeActionDate.PartnerPaysBenefits ||
                        consultantAssignedHistoryToCreate.HourlyClientRate != recentHistoryBeforeActionDate.HourlyClientRate ||
                        consultantAssignedHistoryToCreate.MonthlyClientRate != recentHistoryBeforeActionDate.MonthlyClientRate ||
                        consultantAssignedHistoryToCreate.MonthlyClientRateNumDays != recentHistoryBeforeActionDate.MonthlyClientRateNumDays ||
                        consultantAssignedHistoryToCreate.AccessToTrackingTool != recentHistoryBeforeActionDate.AccessToTrackingTool ||
                        consultantAssignedHistoryToCreate.HolidaysMustBePaid != recentHistoryBeforeActionDate.HolidaysMustBePaid ||
                        consultantAssignedHistoryToCreate.IsDefaultProject != recentHistoryBeforeActionDate.IsDefaultProject ||
                        consultantAssignedHistoryToCreate.ParticipatesInOnCalls != recentHistoryBeforeActionDate.ParticipatesInOnCalls ||
                        consultantAssignedHistoryToCreate.NumHoursForHoliday != recentHistoryBeforeActionDate.NumHoursForHoliday ||
                        consultantAssignedHistoryToCreate.PrimaryReportTrackingToolName != recentHistoryBeforeActionDate.PrimaryReportTrackingToolName ||
                        consultantAssignedHistoryToCreate.SecondReportTrackingToolName != recentHistoryBeforeActionDate.SecondReportTrackingToolName
                        )
                    {
                        await _db.PROJECTS_CONSULTANTS_ASSIGNED_HISTORY.AddAsync(consultantAssignedHistoryToCreate);
                        await _db.SaveChangesAsync();
                        messageSuccess = "The Consultant parameters were updated successfully!";
                    }
                    else
                    {
                        messageSuccess = "You did not make any changes!";
                    }
                }

                await transaction.CommitAsync();
                return MethodResponse.CreateSuccessResponse(messageSuccess);
            }
            catch (Exception ex)
            {
                return new MethodResponse { MessageType = "Exception Error", Success = false, Message = ex.Message };
            }
        }

    }
}
