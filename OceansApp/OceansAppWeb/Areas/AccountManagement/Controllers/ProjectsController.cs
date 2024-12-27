using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.Models;
using OceansApp.Models.ViewModels.Components;
using OceansApp.Models.ViewModels.Projects;
using OceansApp.Utility.SharedMethods.InputValidations;
using System.Reflection;
using System.Security.Claims;

namespace OceansAppWeb.Areas.AccountManagement.Controllers
{
    [ApiController]
    [Route("AccountManagement/[controller]")]
    [Area("AccountManagement")]
    [ServiceFilter(typeof(RequireTwoFactorEnabledAttribute))]
    [Authorize]
    [Authorize(Policy = "AccessToProjectsPage")]
    public class ProjectsController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuthorizationService _authorizationService;
        public ProjectsController(IUnitOfWork unitOrWork, IAuthorizationService authorizationService)
        {
            _unitOfWork = unitOrWork;
            _authorizationService = authorizationService;
        }
        [ApiExplorerSettings(IgnoreApi = true)]
        [Authorize(Policy = "AccessToProjectsPage")]
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [Authorize(Policy = "AccessToProjectsPage")]
        [HttpGet("GetProjectsList")]
        public async Task<IActionResult> GetProjectsList(string model)
        {
            try
            {
                if (model != "{}")
                {
                    JObject jsonToValidate = JObject.Parse(model);
                    if (jsonToValidate["Filters"] == null || jsonToValidate["PaginationWithoutFilters"] == null)
                    {
                        return BadRequest(new { errors = new[] { "You should pass a valid Json like: {Filters: null, PaginationWithoutFilters:null}" }, result = "errorGet", detail = "The json is invalid." });
                    }
                    else
                    {
                        if (jsonToValidate["Filters"] != null)
                        {
                            ValidateInputs validateInputs = new();
                            //Validate Filter inputs
                            validateInputs.ValidateNotRequiredAndStringLength("SearchText", "Search Text", jsonToValidate["Filters"]["SearchText"].ToString(), 100, ModelState);
                            validateInputs.ValidateDateValidFormat("StartDate", "Start Date", jsonToValidate["Filters"]["StartDate"], ModelState);
                            validateInputs.ValidateDateValidFormat("EndDate", "End Date", jsonToValidate["Filters"]["EndDate"], ModelState);
                            if (!ModelState.IsValid)
                            {
                                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                                              .Select(e => e.ErrorMessage)
                                                              .ToList();
                                return BadRequest(new { MessageType = "Validation Error", message = "Validation Error", result = "error", errors = errors, detail = "Parameters for filters are not correct." });
                            }
                        }
                    }
                }

                ProjectsPaginationFiltersVM projectsPaginationFilters = System.Text.Json.JsonSerializer.Deserialize<ProjectsPaginationFiltersVM>(model);

                ProjectsPaginationFiltersVM paginationFilters = new();
                paginationFilters.Filters = new ProjectsFiltersGetAllVM();

                int numAppliedFilters = 0;
                if (projectsPaginationFilters.Filters != null)
                {
                    foreach (var prop in projectsPaginationFilters.Filters.GetType().GetProperties())
                    {
                        var value = prop.GetValue(projectsPaginationFilters.Filters, null);
                        if (value is not null and not "")
                        {
                            numAppliedFilters++;
                        }
                    }
                }
                var setPagination = new PaginationFiltersBehavior();
                paginationFilters.PaginationWithoutFilters = setPagination.SetPagination(projectsPaginationFilters.PaginationWithoutFilters, numAppliedFilters);

                if (numAppliedFilters > 0)
                {
                    paginationFilters.Filters = projectsPaginationFilters.Filters;
                }

                var totalResults = await _unitOfWork.Project.GetAllProjectsWithFiltersAsync(paginationFilters);
                paginationFilters.PaginationWithoutFilters.Pagination.TotalResults = totalResults.totalCount;

                var data = new { ProjectsList = totalResults.projects, PaginationFilters = paginationFilters };
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(new { errors = new[] { $"There was an error fetching the list of projects." }, success = false, result = "errorGet", detail = ex.Message });
            }
        }

        [Authorize(Policy = "AccessToProjectsPage")]
        [HttpGet("GetProjectDataById")]
        public async Task<IActionResult> GetProjectDataById(int projectId)
        {
            try
            {
                var projectData = await _unitOfWork.Project.GetProjectDataByIdAsync(projectId);
                if (projectData == null)
                {
                    return NotFound(new { error = "The project is not longer in the database."});
                }
                var authToManageAdminitrativeConsultants = await _authorizationService.AuthorizeAsync(User, "AccessToManageAdministrativeConsultants");

                return Ok(new
                {
                    projectData = projectData,
                    allowedManageAdminConsultants = authToManageAdminitrativeConsultants.Succeeded
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = "Error retrieving data. Please report this issue.", detail = ex.Message });
            }
        }

        [Authorize(Policy = "AccessToProjectsPage")]
        [HttpGet("GetAssignedConsultantToProjectById")]
        public async Task<IActionResult> GetAssignedConsultantToProjectById(int consultantProjectAssignedtId)
        {
            try
            {
                var consultantAssignationData = await _unitOfWork.Project.GetAssignedConsultantToProjectById(consultantProjectAssignedtId);
                if (consultantAssignationData == null)
                {
                    return BadRequest(new { error = "The consultant assignation is not longer in the database.", detail = "The consultant assignation was not found in the database." });
                }
                var authToManageAdminitrativeConsultants = await _authorizationService.AuthorizeAsync(User, "AccessToManageAdministrativeConsultants");

                if (!authToManageAdminitrativeConsultants.Succeeded)
                {
                    if (consultantAssignationData.UserCategoryName == "Administrative")
                    {
                        return BadRequest(new { error = "You are not allow to retrieve data from Administrative consultants.", detail = "Without permissions to retrieve data." });
                    }
                }
                return Ok(new
                {
                    consultantAssignation = consultantAssignationData
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = "Error retrieving data. Please report this issue.", detail = ex.Message });
            }
        }

        [Authorize(Policy = "AccessToProjectsPage")]
        [HttpPost("CreateUpdateProject")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUpdateProject([FromBody] CreateUpdateProjectVM projectData)
        {
            try
            {
                if (projectData == null)
                {
                    return BadRequest(new { error = "The object data is null, it should be a valid object.", detail = "Object is null." });
                }
                ValidateInputs validateInputs = new();

                validateInputs.ValidateRequiredAndStringLength("Name", "Project Name", projectData.Name, 150, ModelState);
                validateInputs.ValidateNotRequiredAndStringLength("Description", "Project Description", projectData.Description, 300, ModelState);
                validateInputs.ValidateDateValidFormat("StartDate", "Start Date", projectData.StartDate, ModelState);
                validateInputs.ValidateRequiredFieldAnyValue("StartDate", "Start Date", projectData.StartDate, ModelState);
                validateInputs.ValidateRequiredFieldBooleanType("IsActive", "Is Active", projectData.IsActive, ModelState);
                if (projectData.ProjectType == "E")
                {
                    validateInputs.ValidateRequiredFieldIntType("ClientId", "Client", projectData.ClientId, ModelState);
                    validateInputs.ValidateRequiredFieldBooleanType("IsBillable", "Is Billable", projectData.IsBillable, ModelState);
                }
                else
                {
                    var internalClient = await _unitOfWork.Client.GetFirstOrDefaultAsync(x => x.ClientCode == "OCEADMIN01");
                    if (internalClient == null)
                    {
                        return BadRequest(new { error = "The internal client was not found.", detail = "Client not found." });
                    }
                    projectData.ClientId = internalClient.ClientId;
                    projectData.ClientHasTrackingTool = false;
                    projectData.IsBillable = false;
                }
                validateInputs.ValidateRequiredFieldIntType("SuccessManagerId", "Success Manager", projectData.SuccessManagerId, ModelState);
                validateInputs.ValidateRequiredFieldBooleanType("ClientHasTrackingTool", "Client has tracking tool", projectData.ClientHasTrackingTool, ModelState);

                if (ModelState.IsValid)
                {
                    string userActionedBy = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    var resultMessage = "";
                    projectData.CreatedBy = userActionedBy;
                    int createdProjectId = 0;

                    //IF IS NOT PROJECT ID THEN CREATE THE PROJECT
                    if (projectData.ProjectId == null)
                    {
                        var res = await _unitOfWork.Project.CreateProject(projectData);

                        if (res.Success)
                        {
                            resultMessage = res.Message;
                            createdProjectId = (int)res.IdCreatedElement;
                        }
                        else
                        {
                            if (res.MessageType == "Validation Error")
                            {
                                return BadRequest(new { MessageType = "Validation Error", errors = new[] { res.Message } });
                            }
                            return BadRequest(new { MessageType = res.MessageType, error = res.Message, result = "ErrorSaving", detail = "The Project could be saved." });
                        }
                    }
                    else
                    {
                        //IF IS PROJECT ID THEN UPDATE THE PROJECT
                        var res = await _unitOfWork.Project.UpdateProject(projectData);

                        if (res.Success)
                        {
                            resultMessage = res.Message;
                        }
                        else
                        {
                            if (res.MessageType == "Validation Error")
                            {
                                return BadRequest(new { MessageType = "Validation Error", errors = new[] { res.Message } });
                            }
                            return BadRequest(new { error = res.Message, MessageType = res.MessageType, result = "ErrorSaving", detail = "The Project could be updated." });
                        }
                    }

                    await _unitOfWork.SaveAsync();
                    return Ok(new
                    {
                        success = true,
                        message = resultMessage,
                        projectId = createdProjectId
                    });
                }
                else
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors)
                                                  .Select(e => e.ErrorMessage)
                                                  .ToList();
                    return BadRequest(new { MessageType = "Validation Error", message = "Validation Error", result = "error", errors = errors });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { MessageType = "Exception Error", error = $"There was an error saving the changes. More details: " + ex.Message, detail = ex.Message });
            }
        }

        [Authorize(Policy = "AccessToProjectsPage")]
        [HttpPost("AddUpdateConsultantInProjet")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddUpdateConsultantInProjet([FromBody] CreateUpdateProjectConsultantHistoryVM consultantAssignationData)
        {
            try
            {
                if (consultantAssignationData == null)
                {
                    return BadRequest(new { MessageType = "Exception Error", error = $"The object data is null, it should be a valid object" });
                }
                ValidateInputs validateInputs = new();

                validateInputs.ValidateRequiredFieldIntType("ConsultantId", "Consultant", consultantAssignationData.ConsultantId, ModelState);
                validateInputs.ValidateRequiredFieldIntType("ProjectId", "ProjectId", consultantAssignationData.ProjectId, ModelState);
                validateInputs.ValidateRequiredFieldIntType("PositionId", "Position", consultantAssignationData.PositionId, ModelState);

                validateInputs.ValidateNotRequiredAndGreaterThanZeroFieldNumberValue("HourlyClientRate", "Hourly Client Rate", consultantAssignationData.HourlyClientRate, ModelState);
                validateInputs.ValidateNotRequiredAndGreaterThanZeroFieldNumberValue("HourlySalary", "Hourly Salary", consultantAssignationData.HourlySalary, ModelState);
                validateInputs.ValidateNotRequiredAndGreaterThanZeroFieldNumberValue("MonthlyClientRate", "Monthly Client Rate", consultantAssignationData.MonthlyClientRate, ModelState);
                validateInputs.ValidateNotRequiredAndGreaterThanZeroFieldNumberValue("MonthlySalary", "Monthly Salary", consultantAssignationData.MonthlySalary, ModelState);
                validateInputs.ValidateNotRequiredAndGreaterThanZeroFieldNumberValue("MonthlySalaryPartner", "Monthly Salary Partner", consultantAssignationData.MonthlySalaryPartner, ModelState);

                validateInputs.ValidateRequiredFieldAnyValue("ActionDate", "Action Date", consultantAssignationData.ActionDate, ModelState);
                validateInputs.ValidateDateValidFormat("ActionDate", "Action Date", consultantAssignationData.ActionDate, ModelState);

                validateInputs.ValidateRequiredFieldBooleanType("HolidaysMustBePaid", "Holidays Must Be Paid", consultantAssignationData.HolidaysMustBePaid, ModelState);
                validateInputs.ValidateRequiredFieldBooleanType("AccessToTrackingTool", "Access To Tracking Tool", consultantAssignationData.AccessToTrackingTool, ModelState);
                validateInputs.ValidateRequiredFieldBooleanType("IsDefaultProject", "Is Default Project", consultantAssignationData.IsDefaultProject, ModelState);
                validateInputs.ValidateRequiredFieldBooleanType("ParticipatesInOnCalls", "Participates In On-Calls", consultantAssignationData.ParticipatesInOnCalls, ModelState);
                validateInputs.ValidateRequiredFieldBooleanType("IsAssigningFirstTime", "IsAssigningFirstTime", consultantAssignationData.IsAssigningFirstTime, ModelState);

                if (ModelState.IsValid)
                {
                    string userActionedBy = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                    consultantAssignationData.UserCreatedBy = userActionedBy;

                    var res = await _unitOfWork.Project.AddUpdateConsultantInProjet(consultantAssignationData);

                    if (res.Success)
                    {
                        return Ok(new
                        {
                            success = true,
                            message = res.Message
                        });
                    }
                    else
                    {
                        if (res.MessageType == "Validation Error")
                        {
                            return BadRequest(new { MessageType = "Validation Error", errors = new[] { res.Message } });
                        }
                        return BadRequest(new { MessageType = res.MessageType, error = res.Message });
                    }
                }
                else
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors)
                                                  .Select(e => e.ErrorMessage)
                                                  .ToList();
                    return BadRequest(new { MessageType = "Validation Error", message = "Validation Error", errors = errors });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { MessageType = "Exception Error", error = $"There was an error saving the changes. More details: " + ex.Message, detail = ex.Message });
            }
        }


        [Authorize(Policy = "AccessToProjectsPage")]
        [HttpPost("ActivateDeactivateConsultantFromProject")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ActivateDeactivateConsultantFromProject([FromForm] int projectConsultantAssignedId,
            [FromForm] DateTime actionDate, [FromForm] bool statusToChange)
        {
            try
            {
                ProjectConsultantAssignedHistory existingConsultantAssignationHistory = new();
                existingConsultantAssignationHistory = await _unitOfWork.ProjectConsultantAssignedHistory
                    .GetFirstOrDefaultAsync(x => x.ProjectConsultantAssignedId == projectConsultantAssignedId &&
                    x.ActionDate <= actionDate,
    orderBy: q => q.OrderByDescending(x => x.ActionDate).ThenByDescending(x => x.Id));
                if (existingConsultantAssignationHistory == null)
                {
                    existingConsultantAssignationHistory = await _unitOfWork.ProjectConsultantAssignedHistory
                    .GetFirstOrDefaultAsync(x => x.ProjectConsultantAssignedId == projectConsultantAssignedId &&
                    x.ActionDate >= actionDate,
    orderBy: q => q.OrderBy(x => x.ActionDate).ThenByDescending(x => x.Id));
                }
                string userActionedBy = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (existingConsultantAssignationHistory.IsActive != statusToChange)
                {
                    ProjectConsultantAssignedHistory historyConsultantToCreate = new();
                    foreach (PropertyInfo property in typeof(ProjectConsultantAssignedHistory).GetProperties())
                    {
                        if (property.Name != "Id")
                        {
                            property.SetValue(historyConsultantToCreate, property.GetValue(existingConsultantAssignationHistory));
                        }
                    }
                    historyConsultantToCreate.IsActive = statusToChange;
                    historyConsultantToCreate.UserIdActionedBy = userActionedBy;
                    historyConsultantToCreate.ActionDate = actionDate;
                    historyConsultantToCreate.CreationDate = DateTime.UtcNow;

                    await _unitOfWork.ProjectConsultantAssignedHistory.AddAsync(historyConsultantToCreate);
                    await _unitOfWork.SaveAsync();
                }
                else
                {
                    return BadRequest(new { error = $"The status of the consultat is already {(existingConsultantAssignationHistory.IsActive ? "Active" : "Inactive")}", MessageType = "Validation Error" });
                }

                var successMessage = "The consultant was " + (statusToChange ? "Activated" : "Deactivated") + " from the project!";

                return Ok(new { success = true, message = successMessage });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = $"There was an error in the server, the consultant status could not be updated.", detail = ex.Message });
            }
        }

        [Authorize(Policy = "AccessToProjectsPage")]
        [HttpPost("ActivateDeactivateProject")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ActivateDeactivateProject([FromForm] int projectId)
        {
            using (var transaction = await _unitOfWork.BeginTranAsync())
            {
                try
                {
                    var project = await _unitOfWork.Project.GetFirstOrDefaultAsync(x => x.ProjectId == projectId);
                    if (project == null)
                    {
                        return BadRequest(new { error = "The Project no longer exist in the database.", MessageType = "No Exists Error" });
                    }

                    project.IsActive = project.IsActive ? false : true;

                    await _unitOfWork.SaveAsync(); 

                    await transaction.CommitAsync(); 

                    var successMessage = "The project " + project.Name + " was " + (project.IsActive ? "Activated" : "Deactivated") + " successfully!";
                    return Ok(new { success = true, message = successMessage });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync(); 
                    return BadRequest(new { error = $"There was an error in the server, the project could not be updated.", detail = ex.Message });
                }
            }
        }


        [Authorize(Policy = "AccessToProjectsPage")]
        [HttpGet("GetProjectConsultantAssignedHistoryById")]
        public async Task<IActionResult> GetProjectConsultantAssignedHistoryById(int projectConsultantAssignedId)
        {
            try
            {
                var authToManageAdminitrativeConsultants = await _authorizationService.AuthorizeAsync(User, "AccessToManageAdministrativeConsultants");
                string? userCategoryName = null;
                if (!authToManageAdminitrativeConsultants.Succeeded)
                {
                    userCategoryName = "Consultant";
                }
                var historyList = await _unitOfWork.ProjectConsultantAssignedHistory.GetProjectConsultantAssignedHistoryByAssignationId(projectConsultantAssignedId, userCategoryName);
                if (historyList.Count == 0)
                {
                    return BadRequest(new { error = "The consultant does not have history or the user does not have permission to retrive the data." });
                }
                return Ok(new
                {
                    HistoryList = historyList
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = $"There was an error fetching the list.", success = false, result = "errorGet", detail = ex.Message });
            }
        }

        [Authorize(Policy = "AccessToActiveProjectsListForSelect")]
        [HttpGet("GetAllActiveProjectsList")]
        public async Task<IActionResult> GetAllActiveProjectsList()
        {
            try
            {
                var activeProjects = await _unitOfWork.Project.GetAllAsync(
    filter: x => x.IsActive == true,
    orderBy: q => q.OrderBy(x => x.Name)
);
                List<GetDataForSelectVM> listToSend = new List<GetDataForSelectVM>();
                foreach (var project in activeProjects)
                {
                    GetDataForSelectVM projectToAdd = new()
                    {
                        Value = project.ProjectId,
                        Text = project.Name
                    };
                    listToSend.Add(projectToAdd);
                }
                return Ok(new
                {
                    Projects = listToSend
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = "Error retrieving data. Please report this issue.", detail = ex.Message });
            }
        }

    }
}
