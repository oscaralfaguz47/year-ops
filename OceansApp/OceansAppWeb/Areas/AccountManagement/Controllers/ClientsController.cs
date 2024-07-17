using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.ViewModels.Clients;
using OceansApp.Models.ViewModels.Components;
using OceansApp.Utility.SharedMethods.InputValidations;
using System.Security.Claims;

namespace OceansAppWeb.Areas.AccountManagement.Controllers
{
    [ApiController]
    [Area("AccountManagement")]
    [EnableCors("AllowSpecificOrigin")]
    [ServiceFilter(typeof(RequireTwoFactorEnabledAttribute))]
    [Authorize]
    [Route("AccountManagement/[controller]")]
    public class ClientsController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public ClientsController(IUnitOfWork unitOrWork)
        {
            _unitOfWork = unitOrWork;
        }

        [Authorize(Policy = "AccessToClientsPage")]
        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [Authorize(Policy = "AccessToClientsPage")]
        [HttpGet("GetClientsList")]
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

                ClientsPaginationFiltersVM clientsPaginationFilters = System.Text.Json.JsonSerializer.Deserialize<ClientsPaginationFiltersVM>(model);

                ClientsPaginationFiltersVM paginationFilters = new();
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

        [Authorize(Policy = "AccessToClientsListForSelect")]
        [HttpGet("GetAllClientsListForSelect")]
        public async Task<IActionResult> GetAllClientsListForSelect()
        {
            try
            {
                var clients = await _unitOfWork.Client.GetAllClientsForSelectAsync();
                return Ok(new
                {
                    Clients = clients
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = "Error retrieving data. Please report this issue.", detail = ex.Message });
            }
        }

        [Authorize(Policy = "AccessToClientsListForSelect")]
        [HttpGet("GetAllActiveClientsListForSelect")]
        public async Task<IActionResult> GetAllActiveClientsListForSelect()
        {
            try
            {
                var clients = await _unitOfWork.Client.GetAllActiveClientsForSelectAsync();
                return Ok(new
                {
                    Clients = clients
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = "Error retrieving data. Please report this issue.", detail = ex.Message });
            }
        }

        [Authorize(Policy = "AccessToClientsPage")]
        [HttpGet("GetClientDataById")]
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

        [Authorize(Policy = "AccessToGetSuccessManagerIdAndNameWhereClientId")]
        [HttpGet("GetSuccessManagerIdAndNameByClientId")]
        public async Task<IActionResult> GetSuccessManagerIdAndNameByClientId(int clientId)
        {
            try
            {
                var successManager = await _unitOfWork.Client.GetSuccessManagerIdAndNameByClientId(clientId);

                return Ok(new
                {
                    successManager = successManager
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = "Error retrieving data. Please report this issue.", detail = ex.Message });
            }
        }

        [Authorize(Policy = "AccessToClientsPage")]
        [HttpPost("CreateUpdateClient")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUpdateClient([FromBody] CreateUpdateClientVM clientData)
        {
            if (clientData == null)
            {
                return BadRequest(new { errors = new[] { "The object data is null, it should be a valid object." }, detail = "Object is null." });
            }
            ValidateInputs validateInputs = new();

            validateInputs.ValidateRequiredAndStringLength("Name", "Name", clientData.Name, 150, ModelState);
            validateInputs.ValidateNotRequiredAndStringLength("Contact", "Stakeholder Name", clientData.Contact, 30, ModelState);
            validateInputs.ValidateNotRequiredAndStringLength("ContactOcuppation", "Stakeholder Title", clientData.ContactOccupation, 30, ModelState);
            validateInputs.ValidateEmail("Emails", "Stakeholder Email", clientData.Emails, ModelState);
            validateInputs.ValidateRequiredAndStringLength("Emails", "Stakeholder Email", clientData.Emails, 249, ModelState);
            validateInputs.ValidateRequiredFieldAnyValue("AdmissionDate", "Admission Date", clientData.AdmissionDate.ToString(), ModelState);
            validateInputs.ValidateDateValidFormat("AdmissionDate", "Admission Date", clientData.AdmissionDate, ModelState);
            validateInputs.ValidateNoNegativeNumber("PaymentCondition", "Payment Condition", Convert.ToDecimal(clientData.PaymentCondition), ModelState);
            validateInputs.ValidateNotRequiredAndStringLength("Address", "Client Address", clientData.Address, 160, ModelState);
            validateInputs.ValidateRequiredFieldIntType("SuccessManagerId", "Success Manager", clientData.SuccessManagerId, ModelState);

            if (clientData.AdditionalEmailsForNotifications != null)
            {
                string[] emails = clientData.AdditionalEmailsForNotifications.Split(new[] { ';', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                var emailList = new List<string>(emails);
                validateInputs.ValidateListOfEmails("AdditionalEmailsForNotifications", "Additional Emails", emailList, ModelState);
            }
            validateInputs.ValidateMinAndMaxLenthNumber("LatePaymentFee", "Late Payment Fee", clientData.LatePaymentFee, 0, 100, ModelState);

            if (ModelState.IsValid)
            {
                try
                {
                    var claimsIdentity = (ClaimsIdentity)User.Identity;
                    var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                    var costaRicaTime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, "Central America Standard Time");

                    var client = await _unitOfWork.Client.GetFirstOrDefaultAsync(x => x.ClientId == clientData.ClientId);
                    if (client == null)
                    {
                        return BadRequest(new { MessageType = "Exception Error", error = $"The client does not exist in the database.", detail = "The client no longer exists." });
                    }
                    client.Name = clientData.Name.Trim();
                    client.Contact = clientData.Contact.Trim();
                    client.ContactOccupation = clientData.ContactOccupation.Trim();
                    client.Emails = clientData.Emails.Trim();
                    client.AdmissionDate = DateTime.Parse(clientData.AdmissionDate);
                    client.PaymentCondition = clientData.PaymentCondition;
                    client.LatePaymentFee = ((decimal)clientData.LatePaymentFee / 100m);
                    client.ClientClass = clientData.ClientClass;
                    client.Address = clientData.Address.Trim();
                    var consultant = await _unitOfWork.ConsultantDetail.GetFirstOrDefaultAsync(x=>x.ConsultantId == clientData.SuccessManagerId);
                    if (consultant == null)
                    {
                        return BadRequest(new { MessageType = "Exception Error", error = $"The Consultant does not exist in the database.", detail = "The Success Manager no longer exists." });
                    }
                    var verifySuccessManager = await _unitOfWork.ConsultantDetail.GetNumOfUsersByCategoryConsultantIdAndPosition("Administrative", "Success Manager", consultant.ConsultantId);
                    if (verifySuccessManager == 0)
                    {
                        return BadRequest(new { MessageType = "Exception Error", error = $"The selected Succes Manager is not a Success Manager.", detail = "Wrong sent data." });
                    }
                    client.SuccessManager = consultant.ConsultantId;
                    client.IsActive = clientData.IsActive;
                    client.AllowSentLatePaymentNotifications = (bool)clientData.AllowSentLatePaymentNotifications;
                    client.AdditionalEmailsForNotifications = clientData.AdditionalEmailsForNotifications;
                    client.DateLastUpdate = costaRicaTime;

                    await _unitOfWork.SaveAsync();
                    return Ok(new
                    {
                        success = true,
                        message = $"The client {client.Name} was updated successfully!"
                    });
                }
                catch (Exception ex)
                {
                    return BadRequest(new { MessageType = "Exception Error", error = $"There was an error updating the Client. More details: " + ex.Message, detail = ex.Message });
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

        [Authorize(Policy = "AccessToClientsPage")]
        [HttpPost("ActivateDeactivateClient")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ActivateDeactivateClient([FromForm] int clientId)
        {
            try
            {
                var client = await _unitOfWork.Client.GetFirstOrDefaultAsync(x => x.ClientId == clientId);
                if (client == null)
                {
                    return BadRequest(new { error = "The Client no longer exist in the database.", MessageType = "No Exists Error" });
                }
                client.IsActive = client.IsActive == "S" ? "N" : "S";
                await _unitOfWork.SaveAsync();

                var successMessage = "The client " + client.Name + " was " + (client.IsActive == "S" ? "Activated" : "Deactivated") + " successfully!";

                return Ok(new { success = true, message = successMessage });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = $"There was an error in the server, the client status could not be updated.", detail = ex.Message });
            }
        }

        [Authorize(Policy = "AccessToClientsPage")]
        [HttpPost("ActivateDeactivateNotifications")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ActivateDeactivateNotifications([FromForm] int clientId)
        {
            try
            {
                var client = await _unitOfWork.Client.GetFirstOrDefaultAsync(x => x.ClientId == clientId);
                if (client == null)
                {
                    return BadRequest(new { error = "The Client no longer exist in the database.", MessageType = "No Exists Error" });
                }
                client.AllowSentLatePaymentNotifications = client.AllowSentLatePaymentNotifications == true ? false : true;
                await _unitOfWork.SaveAsync();

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
