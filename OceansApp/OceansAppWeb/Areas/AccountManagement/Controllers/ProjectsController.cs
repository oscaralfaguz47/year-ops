using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.Models;
using OceansApp.Models.ViewModels.Components;
using OceansApp.Models.ViewModels.Projects;
using OceansApp.Utility.SharedMethods.InputValidations;
using System.Security.Claims;

namespace OceansAppWeb.Areas.AccountManagement.Controllers
{
    [Area("AccountManagement")]
    [RequireTwoFactorEnabled]
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
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
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

        [HttpGet]
        public async Task<IActionResult> GetProjectDataById(int projectId)
        {
            try
            {
                var projectData = await _unitOfWork.Project.GetProjectDataById(projectId);
                if (projectData == null)
                {
                    return BadRequest(new { error = "The project is not longer in the database.", detail = "The project was not found in the database." });
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

        [HttpGet]
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

        [HttpPost]
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
                validateInputs.ValidateRequiredFieldBooleanType("IsActive", "Is Active", projectData.IsActive, ModelState);
                if (projectData.ProjectType == "E")
                {
                    validateInputs.ValidateRequiredFieldIntType("ClientId", "Client", projectData.ClientId, ModelState);
                    validateInputs.ValidateRequiredFieldBooleanType("IsBillable", "Is Billable", projectData.IsBillable, ModelState);
                }
                else
                {
                    var internalClient = _unitOfWork.Client.GetFirstOrDefault(x => x.ClientCode == "OCEADMIN01");
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

                if (projectData.AssignedConsultants != null)
                {
                    HashSet<int> existingConsultantIds = new HashSet<int>();

                    foreach (var consultant in projectData.AssignedConsultants)
                    {
                        if (!existingConsultantIds.Add(consultant.ConsultantId))
                        {
                            ModelState.AddModelError("ConsultantId", $"You are adding duplicated consultants in the list.");
                            continue;
                        }
                        if (consultant.ProjectConsultantAssignedId == null)
                        {
                            validateInputs.ValidateDateValidFormat("ActionDate", "Action Date", consultant.ActionDate, ModelState);
                        }
                        validateInputs.ValidateRequiredFieldIntType("ConsultantId", "Consultant", consultant.ConsultantId, ModelState);
                        validateInputs.ValidateRequiredAndStringLength("PositionDetail", "Position Description", consultant.PositionDetail, 130, ModelState);
                    }
                }

                if (ModelState.IsValid)
                {
                    var claimsIdentity = (ClaimsIdentity)User.Identity;
                    var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                    var costaRicaTime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, "Central America Standard Time");
                    var resultMessage = "";
                    projectData.CreatedBy = claim.Value;
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
                            return BadRequest(new { error = res.Message, MessageType = res.MessageType, result = "ErrorSaving", detail = "The Project could be updated." });
                        }
                    }

                    _unitOfWork.Save();
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateConsultantParameters([FromBody] CreateUpdateProjectConsultantAssignedVM consultantParametersData)
        {
            try
            {
                if (consultantParametersData == null)
                {
                    return BadRequest(new { error = "The object data is null, it should be a valid object.", detail = "Object is null." });
                }
                ValidateInputs validateInputs = new();

                validateInputs.ValidateDateValidFormat("ActionDate", "Action Date", consultantParametersData.ActionDate, ModelState);
                validateInputs.ValidateRequiredFieldIntType("ProjectConsultantAssignedId", "Project Consultant Assigned Id", consultantParametersData.ProjectConsultantAssignedId, ModelState);
                validateInputs.ValidateNotRequiredAndStringLength("PositionDetail", "Position Description", consultantParametersData.PositionDetail, 130, ModelState);

                if (ModelState.IsValid)
                {
                    var claimsIdentity = (ClaimsIdentity)User.Identity;
                    var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                    var resultMessage = "";

                    var res = await _unitOfWork.Project.UpdateConsultantAssignedParameters(consultantParametersData, claim.Value);

                    if (res.Success)
                    {
                        resultMessage = res.Message;
                    }
                    else
                    {
                        return BadRequest(new { error = res.Message, MessageType = res.MessageType, result = "ErrorSaving", detail = "The Consultant parameters could be updated." });
                    }
                    _unitOfWork.Save();
                    return Ok(new
                    {
                        success = true,
                        message = resultMessage
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ActivateDeactivateConsultantFromProject(int projectConsultantAssignedId, DateTime actionDate)
        {
            try
            {
                var consultantAssignation = _unitOfWork.ProjectConsultantAssigned.GetFirstOrDefault(x => x.ProjectConsultantAssignedId == projectConsultantAssignedId);
                if (consultantAssignation == null)
                {
                    return BadRequest(new { error = "The Consultant assignation no longer exist in the database.", MessageType = "No Exists Error" });
                }
                var actionDescription = consultantAssignation.IsActive ? "Consultant Deactivated" : "Consultant Activated";
                var action = _unitOfWork.ProjectConsultantAssignedHistoryAction.GetFirstOrDefault(x => x.Name == actionDescription);
                var costaRicaTime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, "Central America Standard Time");
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                var userActionedBy = _unitOfWork.ConsultantDetail.GetFirstOrDefault(x => x.UserId == claim.Value);
                if (consultantAssignation.IsActive)
                {
                    ProjectConsultantAssignedHistory historyConsultant = new()
                    {
                        ProjectConsultantAssignedId = projectConsultantAssignedId,
                        ActionId = action.ActionId,
                        ActionDate = actionDate,
                        CreationDate = costaRicaTime,
                        UserActionedBy = userActionedBy.ConsultantId,
                        NewValue = 0,
                        OldValue = 1
                    };
                    _unitOfWork.ProjectConsultantAssignedHistory.Add(historyConsultant);
                }
                else
                {
                    ProjectConsultantAssignedHistory historyConsultant = new()
                    {
                        ProjectConsultantAssignedId = projectConsultantAssignedId,
                        ActionId = action.ActionId,
                        ActionDate = actionDate,
                        CreationDate = costaRicaTime,
                        UserActionedBy = userActionedBy.ConsultantId,
                        NewValue = 1,
                        OldValue = 0
                    };
                    _unitOfWork.ProjectConsultantAssignedHistory.Add(historyConsultant);
                }
                consultantAssignation.IsActive = consultantAssignation.IsActive ? false : true;
                _unitOfWork.Save();

                var successMessage = "The consultant was " + (consultantAssignation.IsActive ? "Activated" : "Deactivated") + " from the project!";

                return Ok(new { success = true, message = successMessage });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = $"There was an error in the server, the consultant assignation could not be updated.", detail = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ActivateDeactivateProject(int projectId)
        {
            try
            {
                var project = _unitOfWork.Project.GetFirstOrDefault(x => x.ProjectId == projectId);
                if (project == null)
                {
                    return BadRequest(new { error = "The Project no longer exist in the database.", MessageType = "No Exists Error" });
                }
                project.IsActive = project.IsActive ? false : true;
                _unitOfWork.Save();
                var successMessage = "The project " + project.Name + " was " + (project.IsActive ? "Activated" : "Deactivated") + " successfully!";

                return Ok(new { success = true, message = successMessage });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = $"There was an error in the server, the project could not be updated.", detail = ex.Message });
            }
        }

        [HttpGet]
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
                var historyList = _unitOfWork.ProjectConsultantAssignedHistory.GetProjectConsultantAssignedHistoryByAssignationId(projectConsultantAssignedId, userCategoryName);
                if (historyList.Result.Count == 0)
                {
                    return BadRequest(new { error = "The consultant does not have history or the user does not have permission to retrive the data."});
                }
                return Ok(new
                {
                    HistoryList = historyList
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error =  $"There was an error fetching the list.", success = false, result = "errorGet", detail = ex.Message });
            }
        }

    }
}
