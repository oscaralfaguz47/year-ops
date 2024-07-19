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

        public async Task<CreateUpdateProjectVM> GetProjectDataById(int projectId)
        {
            var connection = _db.Database.GetDbConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@ProjectId", projectId);

            using (var multiResultSet = await connection.QueryMultipleAsync("SP_PROJECTS_GetProjectDataById", parameters, commandType: CommandType.StoredProcedure))
            {
                var project = await multiResultSet.ReadFirstOrDefaultAsync<CreateUpdateProjectVM>();
                var assignedConsultants = await multiResultSet.ReadAsync<CreateUpdateProjectConsultantHistoryVM>();

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
                    AssignedConsultants = (List<GetConsultantsAssignedToProjectVM>)assignedConsultants
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

                await _db.SaveChangesAsync();
                await transaction.CommitAsync();
                return new MethodResponse { Success = true, Message = $"The Project {existingProject.Name} was updated successfully." };
            }
            catch (Exception ex)
            {
                return new MethodResponse { MessageType = "Exception Error", Success = false, Message = ex.Message };
            }
        }

        public async Task<MethodResponse> AddUpdateConsultantInProjet(CreateUpdateProjectConsultantHistoryVM consultantAssignationData)
        {
            try
            {
                var messageSuccess = "";
                var existingProject = await _db.PROJECTS.FirstOrDefaultAsync(x => x.ProjectId == consultantAssignationData.ProjectId);
                if (existingProject != null)
                {
                    return MethodResponse.CreateFailureValidationResponse("The project you are trying to add the consultant to no longer exists.", "ProjectId");
                }

                var existingConsultantAssignation = await _db.PROJECTS_CONSULTANTS_ASSIGNED
                    .FirstOrDefaultAsync(x => x.ConsultantId == consultantAssignationData.ConsultantId &&
                    x.ProjectId == consultantAssignationData.ProjectId);

                if (existingConsultantAssignation != null && (bool)consultantAssignationData.IsAssigningFirstTime)
                {
                    return MethodResponse.CreateFailureValidationResponse("The consultant you are trying to add to this project is already added.", "IsAssigningFirstTime");
                }

                using var transaction = await _db.Database.BeginTransactionAsync();

                var projectAssignations = await _db.PROJECTS_CONSULTANTS_ASSIGNED.Where(x => x.ConsultantId == consultantAssignationData.ConsultantId).ToListAsync();
                var defaultProject = false;
                if (projectAssignations.Count == 0 || (bool)consultantAssignationData.IsDefaultProject)
                {
                    defaultProject = true;
                }

                if (projectAssignations.Count > 0 && (bool)consultantAssignationData.IsDefaultProject)
                {
                    var projectAssignationsHistory = await _db.PROJECTS_CONSULTANTS_ASSIGNED_HISTORY
                        .Where(x => x.ProjectConsultantAssignedId == projectAssignations[0].ProjectConsultantAssignedId
                        && x.ActionDate >= consultantAssignationData.ActionDate).ToListAsync();

                    foreach (var projAssignHistory in projectAssignationsHistory)
                    {
                        projAssignHistory.IsDefaultProject = false;
                    }
                    await _db.SaveChangesAsync();
                }

                ProjectConsultantAssignedHistory consultantAssignedHistoryToCreate = new()
                {
                    HourlyClientRate = consultantAssignationData.HourlyClientRate,
                    MonthlyClientRate = consultantAssignationData.MonthlyClientRate,
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
                    CreationDate = DateTime.UtcNow
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
                        .OrderBy(x => x.ActionDate)
                        .FirstOrDefaultAsync();

                    consultantAssignedHistoryToCreate.ProjectConsultantAssignedId = existingConsultantAssignation.ProjectConsultantAssignedId;
                    consultantAssignedHistoryToCreate.IsActive = recentHistoryBeforeActionDate.IsActive;

                    if (HasDifferences(consultantAssignedHistoryToCreate, recentHistoryBeforeActionDate))
                    {
                        await _db.PROJECTS_CONSULTANTS_ASSIGNED_HISTORY.AddAsync(consultantAssignedHistoryToCreate);
                        await _db.SaveChangesAsync();
                    }

                    messageSuccess = "The Consultant parameters were updated successfully!";
                }

                await transaction.CommitAsync();
                return new MethodResponse { Success = true, Message = messageSuccess };
            }
            catch (Exception ex)
            {
                return new MethodResponse { MessageType = "Exception Error", Success = false, Message = ex.Message };
            }
        }
        private bool HasDifferences(ProjectConsultantAssignedHistory newItem, ProjectConsultantAssignedHistory existingItem)
        {
            var properties = typeof(ProjectConsultantAssignedHistory).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var property in properties)
            {
                var newValue = property.GetValue(newItem);
                var existingValue = property.GetValue(existingItem);
                if (property.GetValue(newItem) != property.GetValue(existingItem))
                {
                    if (newValue == null && existingValue != null || newValue != null && !newValue.Equals(existingValue))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

    }
}
