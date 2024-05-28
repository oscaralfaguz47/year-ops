using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.ViewModels.Components;
using OceansApp.Models.ViewModels.ConsultantReimbursedBenefits;
using OceansApp.Utility.SharedMethods.InputValidations;
using System.Security.Claims;

namespace OceansAppWeb.Areas.General.Controllers
{
    [Area("General")]
    [Authorize]
    [Authorize(Policy = "AccessToManageConsultantReimbursedBenefits")]
    [RequireTwoFactorEnabled]
    public class ConsultantReimbursedBenefitsController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _config;
        public ConsultantReimbursedBenefitsController(IUnitOfWork unitOrWork, IConfiguration config)
        {
            _unitOfWork = unitOrWork;
            _config = config;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetConsultantReimbursedBenefitsList(string model)
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

                ConsultantReimbursedBenefitsPaginationFiltersVM consultantReimbursedBenefitsPaginationFilters = System.Text.Json.JsonSerializer.Deserialize<ConsultantReimbursedBenefitsPaginationFiltersVM>(model);

                ConsultantReimbursedBenefitsPaginationFiltersVM paginationFilters = new();
                paginationFilters.Filters = new ConsultantReimbursedBenefitsFiltersGetAllVM();

                int numAppliedFilters = 0;
                if (consultantReimbursedBenefitsPaginationFilters.Filters != null)
                {
                    foreach (var prop in consultantReimbursedBenefitsPaginationFilters.Filters.GetType().GetProperties())
                    {
                        var value = prop.GetValue(consultantReimbursedBenefitsPaginationFilters.Filters, null);
                        if (value is not null and not "")
                        {
                            numAppliedFilters++;
                        }
                    }
                }
                var setPagination = new PaginationFiltersBehavior();
                paginationFilters.PaginationWithoutFilters = setPagination.SetPagination(consultantReimbursedBenefitsPaginationFilters.PaginationWithoutFilters, numAppliedFilters);

                if (numAppliedFilters > 0)
                {
                    paginationFilters.Filters = consultantReimbursedBenefitsPaginationFilters.Filters;
                }

                var totalResults = await _unitOfWork.ConsultantReimbursedBenefit.GetAllConsultantsReimbursedBenefitsWithFiltersAsync(paginationFilters);
                paginationFilters.PaginationWithoutFilters.Pagination.TotalResults = totalResults.totalCount;

                var data = new { ReimbursedBenefitsList = totalResults.reimbursedBenefits, PaginationFilters = paginationFilters };
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(new { errors = new[] { $"There was an error fetching the list of reimbursed benefits." }, success = false, result = "errorGet", detail = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUpdateBenefitReimbursement([FromBody] CreateUpdateConsultantBenefitReimbursementVM benefitReimbursementData)
        {
            try
            {
                if (benefitReimbursementData == null)
                {
                    return BadRequest(new { error = "The object data is null, it should be a valid object.", detail = "Object is null." });
                }
                ValidateInputs validateInputs = new();

                validateInputs.ValidateNonRequiredFieldIntType("ReimbursedBenefitId", "Reimbursed Benefit Id", benefitReimbursementData.ReimbursedBenefitId, ModelState);
                validateInputs.ValidateRequiredFieldIntType("BenefitId", "Benefit", benefitReimbursementData.BenefitId, ModelState);
                validateInputs.ValidateNotRequiredAndStringLength("Detail", "Detail", benefitReimbursementData.Detail, 150, ModelState);
                validateInputs.ValidateRequiredFieldIntType("ConsultantId", "Consultant", benefitReimbursementData.ConsultantId, ModelState);
                validateInputs.ValidateRequiredFieldNumberValue("AmountReimbursed", "Amount to Reimburse", benefitReimbursementData.AmountReimbursed, ModelState);
                validateInputs.ValidateNoNegativeNumber("AmountReimbursed", "Amount to Reimburse", benefitReimbursementData.AmountReimbursed, ModelState);
                validateInputs.ValidateNumberLessOrEqualThanZero("AmountReimbursed", "Amount to Reimburse", benefitReimbursementData.AmountReimbursed, ModelState);
                validateInputs.ValidateDateValidFormat("DateToBeReimbursed", "Reimbursement Date", benefitReimbursementData.DateToBeReimbursed, ModelState);
                validateInputs.ValidateRequiredFieldAnyValue("DateToBeReimbursed", "Reimbursement Date", benefitReimbursementData.DateToBeReimbursed, ModelState);
                validateInputs.ValidateRequiredFieldIntType("BenefitCategoryId", "Benefit Category", benefitReimbursementData.BenefitCategoryId, ModelState);

                if (ModelState.IsValid)
                {
                    var claimsIdentity = (ClaimsIdentity)User.Identity;
                    var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                    var resultMessage = "";
                    var userActionedBy = claim.Value;

                    //IF IS NOT BENEFIT REIMBURSEMENT ID THEN CREATE IT
                    if (benefitReimbursementData.ReimbursedBenefitId == null)
                    {
                        var res = await _unitOfWork.ConsultantReimbursedBenefit.CreateBenefitReimbursement(userActionedBy, benefitReimbursementData);

                        if (res.Success)
                        {
                            resultMessage = res.Message;
                        }
                        else
                        {
                            if (res.MessageType != "Validation Error")
                            {
                                return BadRequest(new { MessageType = res.MessageType, error = res.Message, result = "ErrorSaving", detail = "The Benefit Reimbursement could be saved." });
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
                        //IF IS ID THEN UPDATE THE BENEFIT REIMBURSEMENT
                        var res = await _unitOfWork.ConsultantReimbursedBenefit.UpdateBenefitReimbursement(userActionedBy, benefitReimbursementData);
                        if (res.Success)
                        {
                            resultMessage = res.Message;
                        }
                        else
                        {
                            if (res.MessageType != "Validation Error")
                            {
                                return BadRequest(new { error = res.Message, MessageType = res.MessageType, result = "ErrorSaving", detail = "The Benefit Reimbursement could be updated." });
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
                return BadRequest(new { MessageType = "Exception Error", error = $"There was an error saving the changes. More details: " + ex.Message, detail = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetBenefitReimbursementDataById(int benefitReimbursementId)
        {
            try
            {
                var benefitReimbursementData = await _unitOfWork.ConsultantReimbursedBenefit.GetBenefitReimbursementDataById(benefitReimbursementId);
                if (benefitReimbursementData == null)
                {
                    return BadRequest(new { error = "The Benefit Reimbursement is not longer in the database." });
                }

                return Ok(new
                {
                    benefitReimbursementData = benefitReimbursementData
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = "Error retrieving data. Please report this issue.", detail = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectBenefitReimbursement(int benefitReimbursementId)
        {
            try
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                var res = await _unitOfWork.ConsultantReimbursedBenefit.RejectBenefitReimbursement(claim.Value, benefitReimbursementId);
                if (res.Success)
                {
                    return Ok(new { success = true, message = res.Message });
                }
                else
                {
                    return BadRequest(new { error = res.Message, MessageType = res.MessageType, result = "ErrorSaving", detail = "The Benefit Reimbursement could not be rejected." });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = $"There was an error in the server, the benefit reimbursement could not be rejected.", result = "ErrorDeleting", detail = ex.Message });
            }
        }

    }
}
