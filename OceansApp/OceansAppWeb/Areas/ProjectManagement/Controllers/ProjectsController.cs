using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.ViewModels.Components;
using OceansApp.Models.ViewModels.Projects;
using OceansApp.Utility.SharedMethods.InputValidations;
using System.Security.Claims;

namespace OceansAppWeb.Areas.ProjectManagement.Controllers
{
    [Area("ProjectManagement")]
    [RequireTwoFactorEnabled]
    [Authorize(Policy = "AccessToProjectsPage")]
    public class ProjectsController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public ProjectsController(IUnitOfWork unitOrWork)
        {
            _unitOfWork = unitOrWork;
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
                            validateInputs.ValidateDateValidFormat("StartDate", "Start Date", jsonToValidate["Filters"]["StartDate"].ToString(), ModelState);
                            validateInputs.ValidateDateValidFormat("EndDate", "End Date", jsonToValidate["Filters"]["EndDate"].ToString(), ModelState);
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

                return Ok(new
                {
                    projectData = projectData
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
                validateInputs.ValidateRequiredFieldBooleanType("IsBillable", "Is Billable", projectData.IsBillable, ModelState);
                validateInputs.ValidateRequiredFieldIntType("ClientId", "Client", projectData.ClientId, ModelState);
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

                    //IF IS NOT PROJECT ID THEN CREATE THE PROJECT
                    if (projectData.ProjectId == null)
                    {
                        var res = await _unitOfWork.Project.CreateProjectWithAssignedConsultants(projectData);

                        if (res.Success)
                        {
                            resultMessage = res.Message;
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
                        message = $"The project {projectData.Name} was updated successfully!"
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

    }
}
