using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.Models;
using OceansApp.Models.ViewModels;
using OceansApp.Models.ViewModels.Components;
using OceansApp.Models.ViewModels.Consultants;
using OceansApp.Utility.NotificationTemplates;
using OceansApp.Utility.SharedMethods;
using OceansApp.Utility.SharedMethods.InputValidations;
using System.Security.Claims;

namespace OceansAppWeb.Areas.General.Controllers
{
    [Area("General")]
    [RequireTwoFactorEnabled]
    [Authorize]
    [Authorize(Policy = "AccessToViewNoSensitiveInfoForAllConsultants")]
    public class ConsultantsController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _config;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IAuthorizationService _authorizationService;
        private readonly IBackgroundTaskQueue _backgroundTaskQueue;
        public ConsultantsController(IUnitOfWork unitOrWork, IConfiguration config, UserManager<IdentityUser> userManager, 
            IAuthorizationService authorizationService,
            IBackgroundTaskQueue backgroundTaskQueue)
        {
            _unitOfWork = unitOrWork;
            _config = config;
            _userManager = userManager;
            _authorizationService = authorizationService;
            _backgroundTaskQueue = backgroundTaskQueue;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetConsultantsList(string model)
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
                                return BadRequest(new { MessageType = "Validation Error", message = "Validation Error", result = "error", errors = errors, detail = "Parameters for filters are not correct." });
                            }
                        }
                    }
                }

                ConsultantsPaginationFiltersVM consultantsPaginationFilters = System.Text.Json.JsonSerializer.Deserialize<ConsultantsPaginationFiltersVM>(model);

                ConsultantsPaginationFiltersVM paginationFilters = new();
                paginationFilters.Filters = new ConsultantsFiltersGetAllVM();

                int numAppliedFilters = 0;
                if (consultantsPaginationFilters.Filters != null)
                {
                    foreach (var prop in consultantsPaginationFilters.Filters.GetType().GetProperties())
                    {
                        var value = prop.GetValue(consultantsPaginationFilters.Filters, null);
                        if (value is not null and not "")
                        {
                            numAppliedFilters++;
                        }
                    }
                }
                var setPagination = new PaginationFiltersBehavior();
                paginationFilters.PaginationWithoutFilters = setPagination.SetPagination(consultantsPaginationFilters.PaginationWithoutFilters, numAppliedFilters);

                if (numAppliedFilters > 0)
                {
                    paginationFilters.Filters = consultantsPaginationFilters.Filters;
                }

                var totalResults = await _unitOfWork.ConsultantDetail.GetAllConsultantsWithFiltersAsync(paginationFilters);
                paginationFilters.PaginationWithoutFilters.Pagination.TotalResults = totalResults.totalCount;

                var data = new { ConsultantsList = totalResults.consultants, PaginationFilters = paginationFilters };
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(new { errors = new[] { $"There was an error fetching the list of consultants." }, success = false, result = "errorGet", detail = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetConsultantDataById(int consultantId)
        {
            try
            {
                var consultantData = await _unitOfWork.ConsultantDetail.GetConsultantDataById(consultantId);
                if (consultantData == null)
                {
                    return BadRequest(new { error = "The Consultant is not longer in the database.", detail = "The Consultant was not found in the database." });
                }

                return Ok(new
                {
                    consultantData = consultantData
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = "Error retrieving data. Please report this issue.", detail = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUpdateConsultant([FromBody] CreateUpdateConsultantVM consultantData)
        {
            try
            {
                if (consultantData == null)
                {
                    return BadRequest(new { error = "The object data is null, it should be a valid object.", detail = "Object is null." });
                }
                ValidateInputs validateInputs = new();
                var authToManageAdminitrativeConsultants = await _authorizationService.AuthorizeAsync(User, "AccessToManageAdministrativeConsultants");

                validateInputs.ValidateRequiredAndStringLength("Name", "Name", consultantData.Name, 100, ModelState);
                validateInputs.ValidateRequiredAndStringLength("LastName", "Last Name", consultantData.LastName, 150, ModelState);
                validateInputs.ValidateRequiredAndStringLength("Email", "Oceans Email", consultantData.Email, 249, ModelState);
                validateInputs.ValidateEmail("Email", "Oceans Email", consultantData.Email, ModelState);
                if (authToManageAdminitrativeConsultants.Succeeded)
                {
                    validateInputs.ValidateRequiredFieldStringValue("UserCategoryId", "User Category", consultantData.UserCategoryName, ModelState);
                }
                validateInputs.ValidateNotEmptyArray("Positions", "Position", consultantData.Positions, ModelState);
                validateInputs.ValidateRequiredFieldStringValue("IdCountry", "Country", consultantData.IdCountry, ModelState);
                validateInputs.ValidateRequiredFieldStringValue("CompanyId", "Company", consultantData.CompanyId, ModelState);
                validateInputs.ValidateRequiredFieldIntType("PaymentMethodId", "Payment Method", consultantData.PaymentMethodId, ModelState);
                validateInputs.ValidateRequiredFieldIntType("PaymentPeriod", "Payment Period", consultantData.PaymentPeriod, ModelState);
                validateInputs.ValidateNotRequiredAndStringLength("PhoneNumber", "Phone Number", consultantData.PhoneNumber, 100, ModelState);
                validateInputs.ValidateNotRequiredAndStringLength("Phone2", "Phone 2", consultantData.Phone2, 100, ModelState);
                validateInputs.ValidateNotRequiredAndStringLength("Address", "Address", consultantData.Address, 400, ModelState);
                validateInputs.ValidateNotRequiredAndStringLength("PersonalEmail", "Personal Email", consultantData.PersonalEmail, 249, ModelState);
                validateInputs.ValidateEmail("PersonalEmail", "Personal Email", consultantData.PersonalEmail, ModelState);

                if (ModelState.IsValid)
                {
                    var claimsIdentity = (ClaimsIdentity)User.Identity;
                    var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                    var resultMessage = "";
                    var userActionedBy = claim.Value;
                    int createdConsultantId = 0;
                    var callbackurl = "";
                    var code = "";
                    var userRole = "";
                    ApplicationUserCategory? userCategory = null;
                    var isAuthForManageAdminUsers = false;
                    if (authToManageAdminitrativeConsultants.Succeeded)
                    {
                        if (!User.IsInRole("Master") && consultantData.UserRole == "Master")
                        {
                            return BadRequest(new { MessageType = "Exception Error", error = $"You are not authorized to manage the entered role." });
                        }
                        userRole = consultantData.UserRole;
                        isAuthForManageAdminUsers = true;
                        userCategory = _unitOfWork.ApplicationUserCategory.GetFirstOrDefault(x => x.Name == consultantData.UserCategoryName);
                        consultantData.UserCategoryId = userCategory.UserCategoryId;
                        if (userCategory == null)
                        {
                            return BadRequest(new { MessageType = "Not Found", error = $"User category not found.", detail = "The user category was not found." });
                        }
                    }
                    else
                    {
                        userRole = "Computer Consultant";
                        userCategory = _unitOfWork.ApplicationUserCategory.GetFirstOrDefault(x => x.Name == "Consultant");
                        consultantData.UserCategoryId = userCategory.UserCategoryId;
                        if (userCategory == null)
                        {
                            return BadRequest(new { MessageType = "Not Found", error = $"User category not found.", detail = "The user category was not found." });
                        }
                    }
                    //IF IS NOT CONSULTANT ID THEN CREATE THE CONSULTANT
                    if (consultantData.ConsultantId == null)
                    {
                        var user = new ApplicationUser
                        {
                            UserName = consultantData.Email.Trim(),
                            Email = consultantData.Email.Trim(),
                            Name = consultantData.Name.Trim(),
                            LastName = consultantData.LastName.Trim(),
                            IsActive = true,
                            PhoneNumber = consultantData.PhoneNumber,
                            UserCategoryId = userCategory.UserCategoryId
                        };

                        using var transaction = await _unitOfWork.BeginTran();
                        string password = GenerateTokensAndRandomStrings.GeneratePassword();
                        var result = await _userManager.CreateAsync(user, password);

                        if (result.Succeeded)
                        {
                            await _userManager.AddToRoleAsync(user, userRole);

                            code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                            callbackurl = Url.Action("ConfirmEmail", "Account", new { area = "", code = user.Id + ":" + code }, protocol: HttpContext.Request.Scheme);
                        }
                        foreach (var error in result.Errors)
                        {
                            if (error.Code == "DuplicateUserName")
                            {
                                return BadRequest(new { MessageType = "Validation Error", errors = new[] { $"The consultant with email '{consultantData.Email.Trim()}' already exists." }, detail = "Duplication error." });
                            }
                            else
                            {
                                return BadRequest(new { MessageType = "Exception Error", error = $"Something went wrong creating the consultant.", detail = error.Description });
                            }
                        }
                        var res = await _unitOfWork.ConsultantDetail.CreateConsultant(user.Id, userActionedBy, consultantData);

                        if (res.Success)
                        {
                            resultMessage = res.Message;
                            createdConsultantId = (int)res.IdCreatedElement;
                            await transaction.CommitAsync();

                            var createSendNotification = CreateAndSendCreatePasswordEmailNotification(callbackurl, consultantData.Name, consultantData.Email, userActionedBy);
                            if (!createSendNotification.Success)
                            {
                                return BadRequest(new { MessageType = "Exception Error", error = createSendNotification.Message });
                            }
                        }
                        else
                        {
                            return BadRequest(new { MessageType = res.MessageType, error = res.Message, result = "ErrorSaving", detail = "The Consultant could be saved." });
                        }
                    }
                    else
                    {
                        //IF IS CONSULTANT ID THEN UPDATE THE CONSULTANT
                        var res = await _unitOfWork.ConsultantDetail.UpdateUserConsultant(userActionedBy, consultantData, isAuthForManageAdminUsers);
                        if (res.Success)
                        {
                            resultMessage = res.Message;
                        }
                        else
                        {
                            return BadRequest(new { error = res.Message, MessageType = res.MessageType, result = "ErrorSaving", detail = "The Consultant could be updated." });
                        }
                    }
                    return Ok(new
                    {
                        success = true,
                        message = resultMessage,
                        projectId = createdConsultantId
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

        public MethodResponse CreateAndSendCreatePasswordEmailNotification(string callbackUrl, string consultantName, string consultantEmail, string userActionedBy)
        {
            try
            {
                // Prepare email content
                var emailToSend = PrepareEmailContent(callbackUrl, consultantName.Trim(), consultantEmail.Trim());
                var emailNotification = CreateNotification(emailToSend, userActionedBy);

                // Check if notification was successfully created
                if (emailNotification == null || emailNotification.NotificationId <= 0)
                {
                    return new MethodResponse { Success = false, Message = "Failed to create notification." };
                }

                // Queue background task to send email and update notification status
                QueueEmailSendingTask(emailToSend, emailNotification.NotificationId);

                return new MethodResponse { Success = true, Message = "Notification created and email sending queued." };
            }
            catch (Exception ex)
            {
                return new MethodResponse { Success = false, Message = $"An error occurred: {ex.Message}" };
            }
        }

        private SendEmailVM PrepareEmailContent(string callbackUrl, string consultantName, string consultantEmail)
        {
            var emailTemplates = new EmailTemplates();
            var createPassBody = emailTemplates.CreatePasswordBody(callbackUrl, consultantName.Trim());
            var templateEmail = emailTemplates.EmailTemplate("CREATE YOUR PASSWORD", createPassBody);

            return new SendEmailVM
            {
                Subject = "Create your account - Oceans App",
                SharedEmailFrom = Environment.GetEnvironmentVariable(_config["sharedEmailOceansApp"]),
                EmailTo = consultantEmail.Trim(),
                Body = templateEmail
            };
        }

        private Notification CreateNotification(SendEmailVM emailToSend, string userActionedBy)
        {
            var notificatinType = _unitOfWork.NotificationType.GetFirstOrDefault(x => x.Name == "Create new Consultant");

            if (notificatinType == null)
            {
                throw new InvalidOperationException("Notification type 'Create new Consultant' not found.");
            }

            var emailNotification = new Notification
            {
                NotificationTypeId = notificatinType.NotificationTypeId,
                Body = emailToSend.Body,
                Subject = emailToSend.Subject,
                Remitent = emailToSend.SharedEmailFrom,
                SentDate = DateTime.UtcNow,
                SentByUser = userActionedBy
            };
            _unitOfWork.Notification.Add(emailNotification);
            _unitOfWork.Save();
            var notificationMedia = _unitOfWork.NotificationMedia.GetFirstOrDefault(x => x.Name == "Email");
            var recipientUser = _unitOfWork.ApplicationUser.GetFirstOrDefault(x => x.Email == emailToSend.EmailTo);
            var notificationStatus = _unitOfWork.NotificationStatus.GetFirstOrDefault(x => x.Name == "Enviando");
            var notificationRecipient = new NotificationRecipient()
            {
                RecipientMediaInfo = emailToSend.EmailTo,
                NotificationId = emailNotification.NotificationId,
                NotificationMediaId = notificationMedia.NotificationMediaId,
                NotificationStatusId = notificationStatus.NotificationStatusId,
                RecipientUserId = recipientUser?.Id
            };
            _unitOfWork.NotificationRecipient.Add(notificationRecipient);
            _unitOfWork.Save();

            return emailNotification;
        }

        private void QueueEmailSendingTask(SendEmailVM emailToSend, int notificationId)
        {
            _backgroundTaskQueue.QueueBackgroundWorkItem(async (scopeFactory, cancellationToken) =>
            {
                using (var scope = scopeFactory.CreateScope())
                {
                    var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                    var sendEmailService = scope.ServiceProvider.GetRequiredService<ISendEmailRepository>();

                    await SendEmailAndUpdateStatus(unitOfWork, sendEmailService, emailToSend, notificationId, cancellationToken);
                }
            });
        }

        private static async Task SendEmailAndUpdateStatus(IUnitOfWork unitOfWork, ISendEmailRepository sendEmailService, SendEmailVM emailToSend, int notificationId, CancellationToken cancellationToken)
        {
            NotificationStatus notificationStatusForUpdate;

            try
            {
                var emailSent = await sendEmailService.SendEmail(emailToSend);
                notificationStatusForUpdate = unitOfWork.NotificationStatus.GetFirstOrDefault(x => x.Name == "Enviado");
            }
            catch (Exception)
            {
                notificationStatusForUpdate = unitOfWork.NotificationStatus.GetFirstOrDefault(x => x.Name == "Envío fallido");
            }

            if (notificationStatusForUpdate == null)
            {
                throw new InvalidOperationException("Notification status 'Enviado' or 'Envío fallido' not found.");
            }

            var savedNotificationRecipients = unitOfWork.NotificationRecipient.GetAll(x => x.NotificationId == notificationId);

            foreach (var recipient in savedNotificationRecipients)
            {
                recipient.NotificationStatusId = notificationStatusForUpdate.NotificationStatusId;
            }

            unitOfWork.Save();
        }

        [HttpPost]
        [RequireTwoFactorEnabled]
        public async Task<IActionResult> ResentInvite(int consultantId)
        {
            try
            {
                var consultant = _unitOfWork.ConsultantDetail.GetFirstOrDefault(x => x.ConsultantId == consultantId);
                if (consultant == null)
                {
                    return BadRequest(new { MessageType = "Not Found", error = $"The Consultant was not found in the database. " });
                }
                var user = await _userManager.FindByIdAsync(consultant.UserId);
                var userDetails = _unitOfWork.ApplicationUser.GetFirstOrDefault(x => x.Id == user.Id);
                if (userDetails == null)
                {
                    return BadRequest(new { MessageType = "Not Found", error = $"The User was not found in the database. " });
                }
                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var callbackurl = Url.Action("ConfirmEmail", "Account", new { area = "", code = user.Id + ":" + code }, protocol: HttpContext.Request.Scheme);
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                var createAndSendNotification = CreateAndSendCreatePasswordEmailNotification(callbackurl, userDetails.Name, user.Email, claim.Value);
                if (!createAndSendNotification.Success)
                {
                    return BadRequest(new { MessageType = "Exception Error", error = createAndSendNotification.Message });
                }
                return Json(new { success = true, message = $"The invite was successfully sent to " + userDetails.Name + $" " + userDetails.LastName + $"!" });
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost]
        [RequireTwoFactorEnabled]
        public async Task<IActionResult> ResetAuthenticatorFromUser(int consultantId)
        {
            try
            {
                var consultant = _unitOfWork.ConsultantDetail.GetFirstOrDefault(x => x.ConsultantId == consultantId);
                if (consultant == null)
                {
                     return BadRequest(new { MessageType = "Not Found", error = $"The Consultant was not found in the database. " });
                }
                var user = await _userManager.FindByIdAsync(consultant.UserId);
                await _userManager.ResetAuthenticatorKeyAsync(user);
                await _userManager.SetTwoFactorEnabledAsync(user, false);
                return Json(new { success = true, message = "Two-factor authentication was successfully reset!" });
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost]
        public IActionResult ActivateDeactivateConsultantUser(int consultantId)
        {
            try
            {
                var message = "";
                var consultant = _unitOfWork.ConsultantDetail.GetFirstOrDefault(x => x.ConsultantId == consultantId);
                if (consultant == null)
                {
                    return BadRequest(new { MessageType = "Not Found", error = $"The Consultant was not found in the database. " });
                }
                var userToUpdate = _unitOfWork.ApplicationUser.GetFirstOrDefault(x => x.Id == consultant.UserId);
                if (userToUpdate.IsActive == true)
                {
                    userToUpdate.IsActive = false;
                    userToUpdate.LockoutEnd = DateTime.Now.AddYears(1000);
                    message = "The user was successfully deactivated!";
                }
                else
                {
                    userToUpdate.IsActive = true;
                    userToUpdate.LockoutEnd = DateTime.Now.AddDays(-1);
                    message = "The user was successfully activated!";
                }
                _unitOfWork.Save();
                return Json(new { success = true, message = message });
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }


    }
}
