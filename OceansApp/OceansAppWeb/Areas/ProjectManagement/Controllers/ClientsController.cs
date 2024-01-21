using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.ViewModels.Clients;
using OceansApp.Models.ViewModels.Components;
using OceansApp.Utility.SharedMethods;
using System.Security.Claims;

namespace OceansAppWeb.Areas.ProjectManagement.Controllers
{
    [Area("ProjectManagement")]
    [RequireTwoFactorEnabled]
    [Authorize(Policy = "AccessToClientsPage")]
    public class ClientsController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public ClientsController(IUnitOfWork unitOrWork)
        {
            _unitOfWork = unitOrWork;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetClientsList(string model)
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
                            ValidateJsonInputs validateInputs = new();
                            //Validate Filter inputs
                            validateInputs.ValidateAndAddModelError(jsonToValidate["Filters"]["StartDate"].ToString(), "Start Date", ModelState);
                            validateInputs.ValidateAndAddModelError(jsonToValidate["Filters"]["EndDate"].ToString(), "End Date", ModelState);
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

                ClientsPaginationFiltersVM clientsPaginationFilters = System.Text.Json.JsonSerializer.Deserialize<ClientsPaginationFiltersVM>(model);

                ClientsPaginationFiltersVM paginationFilters = new ClientsPaginationFiltersVM();
                paginationFilters.Filters = new ClientsFiltersGetAllVM();

                int numAppliedFilters = 0;
                if (clientsPaginationFilters.Filters != null)
                {
                    foreach (var prop in clientsPaginationFilters.Filters.GetType().GetProperties())
                    {
                        var value = prop.GetValue(clientsPaginationFilters.Filters, null);
                        if (value is not null and not "")
                        {
                            numAppliedFilters++;
                        }
                    }
                }
                var setPagination = new PaginationFiltersBehavior();
                paginationFilters.PaginationWithoutFilters = setPagination.SetPagination(clientsPaginationFilters.PaginationWithoutFilters, numAppliedFilters);

                if (numAppliedFilters > 0)
                {
                    paginationFilters.Filters = clientsPaginationFilters.Filters;
                }

                var totalResults = await _unitOfWork.Client.GetAllClientsWithFiltersAsync(paginationFilters);
                paginationFilters.PaginationWithoutFilters.Pagination.TotalResults = totalResults.totalCount;

                var data = new { ClientsList = totalResults.clients, PaginationFilters = paginationFilters };
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(new { errors = new[] { $"There was an error fetching the list of clients." }, success = false, result = "errorGet", detail = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetClientDataById(int clientId)
        {
            try
            {
                var clientData = await _unitOfWork.Client.GetClientById(clientId);
                if (clientData == null)
                {
                    return BadRequest(new { error = "The client is not longer in the database.", detail = "The client was not found in the database." });
                }

                return Ok(new
                {
                    clientData = clientData
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = "Error retrieving data. Please report this issue.", detail = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUpdateClient([FromBody] CreateUpdateClientVM clientData)
        {
            if (clientData == null)
            {
                return BadRequest(new { errors = new[] { "The object data is null, it should be a valid object." }, detail = "Object is null." });
            }
            if (string.IsNullOrEmpty(clientData.Name) || clientData.Name.Trim().Length > 150)
            {
                ModelState.AddModelError("Name", "The Client Name must be between 1 and 150 characters.");
            }
            if (clientData.Contact.Trim().Length > 30)
            {
                ModelState.AddModelError("Contact", "The Stakeholder Name must be between 1 and 30 characters.");
            }
            if (clientData.ContactOccupation.Trim().Length > 30)
            {
                ModelState.AddModelError("ContactOcuppation", "The Stakeholder Occupation must be between 1 and 30 characters.");
            }
            ValidateData validateData = new();
            if (clientData.Emails != null && clientData.Emails != "")
            {
                if (!validateData.IsValidEmail(clientData.Emails.Trim()))
                {
                    ModelState.AddModelError("Emails", "The Stakeholder Email is not a valid email.");
                }
            }
            ValidateJsonInputs validateInputs = new();

            validateInputs.ValidateAndAddModelError(clientData.AdmissionDate.ToString(), "Admission Date", ModelState);

            if (clientData.PaymentCondition.Trim().Length > 4)
            {
                ModelState.AddModelError("PaymentCondition", "The Payment Condition number must be between 1 and 4 characters.");
            }
            if (Convert.ToDecimal(clientData.PaymentCondition) < 0)
            {
                ModelState.AddModelError("PaymentCondition", "The Payment Condition number can not be a negative number.");
            }
            if (clientData.Emails.Trim().Length > 249)
            {
                ModelState.AddModelError("Emails", "The Stakeholder Email must be between 1 and 249 characters.");
            }
            if (clientData.Address.Trim().Length > 160)
            {
                ModelState.AddModelError("Address", "The Client Address must be between 1 and 160 characters.");
            }
            if (clientData.AdditionalEmailsForNotifications != null)
            {
                string[] emails = clientData.AdditionalEmailsForNotifications.Split(new[] { ';', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                var emailList = new List<string>(emails);
                var count = 0;
                foreach (var email in emailList)
                {
                    count++;
                    if (!validateData.IsValidEmail(email.Trim()))
                    {
                        ModelState.AddModelError("AdditionalEmailsForNotifications", "The email #" + count + " in Additional Emails is not a valid email.");
                    }
                }
            }
            if (clientData.LatePaymentFee < 0 || clientData.LatePaymentFee > 100)
            {
                ModelState.AddModelError("LatePaymentFee", "The Late Payment Fee must be between 0 and 200%.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var claimsIdentity = (ClaimsIdentity)User.Identity;
                    var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                    var resultMessage = "";

                    var costaRicaTime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, "Central America Standard Time");
                    //IF IS NOT HOLIDAY ID THEN CREATE THE HOLIDAY
                    //if (clientData.ConsultantHolidayId == null)
                    //{
                    //    clientData.CreatedBy = claim.Value;

                    //    var res = await _unitOfWork.ConsultantHoliday.CreateHolidayListWithHolidayDates(clientData);

                    //    if (res.Success)
                    //    {
                    //        resultMessage = res.Message;
                    //    }
                    //    else
                    //    {
                    //        return BadRequest(new { MessageType = res.MessageType, errors = new[] { res.Message }, result = "ErrorSaving", detail = "The Holiday list could be saved." });
                    //    }
                    //}
                    //else
                    //{
                    //    //IF IS HOLIDAY ID THEN UPDATE THE HOLIDAY
                    //    var res = await _unitOfWork.ConsultantHoliday.UpdateHolidayListWithHolidayDates(clientData, claim.Value);

                    //    if (res.Success)
                    //    {
                    //        resultMessage = res.Message;
                    //    }
                    //    else
                    //    {
                    //        return BadRequest(new { errors = new[] { res.Message }, MessageType = res.MessageType, result = "ErrorSaving", detail = "The Holiday list could be updated." });
                    //    }
                    //}
                    return Ok(new
                    {
                        success = true,
                        message = resultMessage
                    });
                }
                catch (Exception ex)
                {
                    return BadRequest(new { message = "Error", result = "error", MessageType = "Exception Error", errors = new[] { $"There was an error updating the Client. More details: " + ex.Message } });
                }
            }
            else
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                              .Select(e => e.ErrorMessage)
                                              .ToList();
                return BadRequest(new { MessageType = "Validation Error", message = "Validation Error", result = "error", errors = errors });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ActivateDeactivateClient(int clientId)
        {
            try
            {
                var client = _unitOfWork.Client.GetFirstOrDefault(x => x.ClientId == clientId);
                if (client == null)
                {
                    return BadRequest(new { error = "The Client no longer exist in the database.", MessageType = "No Exists Error" });
                }
                client.IsActive = client.IsActive == "S" ? "N" : "S";
                _unitOfWork.Save();

                var successMessage = "The client " + client.Name + " was " + (client.IsActive == "S" ? "Activated" : "Deactivated") + " successfully!";

                return Ok(new { success = true, message = successMessage });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = $"There was an error in the server, the client status could not be updated.", detail = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ActivateDeactivateNotifications(int clientId)
        {
            try
            {
                var client = _unitOfWork.Client.GetFirstOrDefault(x => x.ClientId == clientId);
                if (client == null)
                {
                    return BadRequest(new { error = "The Client no longer exist in the database.", MessageType = "No Exists Error" });
                }
                client.AllowSentLatePaymentNotifications = client.AllowSentLatePaymentNotifications == true ? false : true;
                _unitOfWork.Save();

                var activeDeactiveStatus = client.AllowSentLatePaymentNotifications ? "Activated" : "Deactivated";
                var successMessage = "The notification for client " + client.Name + " was " + activeDeactiveStatus + " successfully!";

                return Ok(new { success = true, message = successMessage });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = $"There was an error in the server, the client notification status could not be updated.", detail = ex.Message });
            }
        }
    }
}
