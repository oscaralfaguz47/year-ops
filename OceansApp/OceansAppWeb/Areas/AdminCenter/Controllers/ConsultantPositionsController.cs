using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.ViewModels.Components;
using OceansApp.Models.ViewModels.ConsultantPositions;
using OceansApp.Utility.SharedMethods.InputValidations;

namespace OceansAppWeb.Areas.AdminCenter.Controllers
{
    [ApiController]
    [Route("AdminCenter/[controller]")]
    [Area("AdminCenter")]
    [EnableCors("AllowSpecificOrigin")]
    [RequireTwoFactorEnabled]
    [Authorize]
    public class ConsultantPositionsController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public ConsultantPositionsController(IUnitOfWork unitOrWork)
        {
            _unitOfWork = unitOrWork;
        }

        [Authorize(Policy = "AccessToConsultantPositions")]
        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet("GetConsultantPositionsList")]
        public async Task<IActionResult> GetConsultantPositionsList(string model)
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

                            if (!ModelState.IsValid)
                            {
                                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                                              .Select(e => e.ErrorMessage)
                                                              .ToList();
                                return BadRequest(new { MessageType = "Validation Error", message = "Validation Error", result = "error", errors = errors });
                            }
                        }
                    }
                }

                ConsultantPositionsPaginationFiltersVM consultantPositionsPaginationFilters = System.Text.Json.JsonSerializer.Deserialize<ConsultantPositionsPaginationFiltersVM>(model);

                ConsultantPositionsPaginationFiltersVM paginationFilters = new();
                paginationFilters.Filters = new ConsultantPositionsFiltersGetAllVM();

                int numAppliedFilters = 0;
                if (consultantPositionsPaginationFilters.Filters != null)
                {
                    foreach (var prop in consultantPositionsPaginationFilters.Filters.GetType().GetProperties())
                    {
                        var value = prop.GetValue(consultantPositionsPaginationFilters.Filters, null);
                        if (value is not null and not "")
                        {
                            numAppliedFilters++;
                        }
                    }
                }
                var setPagination = new PaginationFiltersBehavior();
                paginationFilters.PaginationWithoutFilters = setPagination.SetPagination(consultantPositionsPaginationFilters.PaginationWithoutFilters, numAppliedFilters);

                if (numAppliedFilters > 0)
                {
                    paginationFilters.Filters = consultantPositionsPaginationFilters.Filters;
                }

                var totalResults = await _unitOfWork.ConsultantPosition.GetAllConsultantPositionsWithFiltersAsync(paginationFilters);
                paginationFilters.PaginationWithoutFilters.Pagination.TotalResults = totalResults.totalCount;

                var data = new { positionsList = totalResults.positions, PaginationFilters = paginationFilters };
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(new { errors = new[] { $"There was an error fetching the list of positions." }, success = false, detail = ex.Message });
            }
        }

        [HttpGet("GetPositionDataById")]
        public async Task<IActionResult> GetPositionDataById(int? positionId)
        {
            try
            {
                var configData = await _unitOfWork.ConsultantPosition.GetCompanyMovementTypesByPositionIdAsync(positionId);
                if (configData == null)
                {
                    return BadRequest(new { error = "Movement types do not exist in the database." });
                }
                string? positionName = null;
                bool? isAdministrative = null;
                if (positionId != null)
                {
                    var existingPosition = await _unitOfWork.ConsultantPosition.GetFirstOrDefaultAsync(x => x.ConsultantPositionId == positionId);
                    if (existingPosition == null)
                    {
                        return BadRequest(new { error = "The position is no longer in the database." });
                    }
                    positionName = existingPosition.Name;
                    isAdministrative = existingPosition.IsAdministrative;
                }
                CreateUpdateConsultantPositionVM modelToSend = new CreateUpdateConsultantPositionVM();
                modelToSend.PositionConfiguration = configData;
                modelToSend.PositionName = positionName;
                modelToSend.IsAdministrative = isAdministrative;

                return Ok(new
                {
                    positionConfigData = modelToSend
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = "Error retrieving data. Please report this issue.", detail = ex.Message });
            }
        }

        [Authorize(Policy = "AccessToAllConsultantPositionsList")]
        [HttpGet("GetAllConsultantPositionsListForSelect")]
        public async Task<IActionResult> GetAllConsultantPositionsListForSelect(bool isAdministrative)
        {
            try
            {
                List<GetDataForSelectVM> positionsList = await _unitOfWork.ConsultantPosition.GetPositionsByIsAdministrative(isAdministrative);

                return Ok(new
                {
                    Positions = positionsList
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = "Error retrieving data. Please report this issue.", detail = ex.Message });
            }
        }

        [Authorize(Policy = "AccessToAllConsultantPositionsList")]
        [HttpGet("GetConsultantPositionsByConsultantId")]
        public async Task<IActionResult> GetConsultantPositionsByConsultantId(int consultantId)
        {
            try
            {
                var positionsList = await _unitOfWork.ConsultantPosition.GetPositionsByConsultantIdAsync(consultantId);

                return Ok(new
                {
                    Positions = positionsList
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = "Error retrieving data. Please report this issue.", detail = ex.Message });
            }
        }

        [HttpPost("CreateUpdateConsultantPosition")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUpdateConsultantPosition([FromBody] CreateUpdateConsultantPositionVM positionConfitData)
        {
            try
            {
                if (positionConfitData == null)
                {
                    return BadRequest(new { error = "The object data is null, it should be a valid object.", detail = "Object is null." });
                }
                ValidateInputs validateInputs = new();

                validateInputs.ValidateRequiredAndStringLength("PositionName", "Position Name", positionConfitData.PositionName, 100, ModelState);
                validateInputs.ValidateRequiredFieldAnyValue("PositionType", "Position Type", positionConfitData.IsAdministrative, ModelState);

                foreach (var positionConfig in positionConfitData.PositionConfiguration)
                {
                    validateInputs.ValidateRequiredFieldIntType("CostCenterId", "Cost Center", positionConfig.CostCenterId, ModelState);
                    validateInputs.ValidateRequiredFieldIntType("AccountingAccountId", "Accounting Account", positionConfig.AccountingAccountId, ModelState);
                    validateInputs.ValidateRequiredFieldIntType("MovementTypeId", "Movement Type", positionConfig.MovementTypeId, ModelState);
                    validateInputs.ValidateRequiredFieldStringValue("CompanyId", "Company", positionConfig.CompanyId, ModelState);
                }

                if (ModelState.IsValid)
                {
                    var resultMessage = "";

                    //IF IS NOT ID THEN CREATE IT
                    if (positionConfitData.PositionId == null)
                    {
                        var res = await _unitOfWork.ConsultantPosition.CreatePositionAsync(positionConfitData);

                        if (res.Success)
                        {
                            resultMessage = res.Message;
                        }
                        else
                        {
                            if (res.MessageType != "Validation Error")
                            {
                                return BadRequest(new { MessageType = res.MessageType, error = res.Message });
                            }
                            else
                            {
                                return BadRequest(new
                                {
                                    MessageType = res.MessageType,
                                    errors = new[] { res.Message }
                                });
                            }

                        }
                    }
                    else
                    {
                        //IF IS ID THEN UPDATE THE DEBIT/CREDIT
                        var res = await _unitOfWork.ConsultantPosition.UpdatePositionAsync(positionConfitData);
                        if (res.Success)
                        {
                            resultMessage = res.Message;
                        }
                        else
                        {
                            if (res.MessageType != "Validation Error")
                            {
                                return BadRequest(new { error = res.Message, MessageType = res.MessageType });
                            }
                            else
                            {
                                return BadRequest(new
                                {
                                    MessageType = res.MessageType,
                                    errors = new[] { res.Message }
                                });
                            }
                        }
                    }
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
                return BadRequest(new { MessageType = "Exception Error", error = $"There was an error saving the changes. More details: " + ex.Message });
            }
        }
    }
}
