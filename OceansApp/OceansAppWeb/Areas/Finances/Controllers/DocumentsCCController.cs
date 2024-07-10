using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.Models;
using OceansApp.Models.ViewModels;
using OceansApp.Models.ViewModels.Components;
using OceansApp.Models.ViewModels.DocumentsCC;
using OceansApp.Models.ViewModels.Notifications;
using OceansApp.Utility.LazyLoading;
using System.Security.Claims;

namespace OceansAppWeb.Areas.Finances.Controllers
{
    [Area("Finances")]
    [ServiceFilter(typeof(RequireTwoFactorEnabledAttribute))]
    [Authorize]
    [Authorize(Policy = "AccessToAccountsReceivable")]
    public class DocumentsCCController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly LazyServiceProvider<ISendEmailRepository> _sendEmail;
        private readonly IConfiguration _config;
        private readonly LazyServiceProvider<ISlackRepository> _slackRepository;
        public DocumentsCCController(IUnitOfWork unitOrWork, LazyServiceProvider<ISendEmailRepository> sendEmail, IConfiguration config,
            LazyServiceProvider<ISlackRepository> slackRepository)
        {
            _unitOfWork = unitOrWork;
            _sendEmail = sendEmail;
            _config = config;
            _slackRepository = slackRepository;
        }

        public async Task<IActionResult> Index(DocumentCCGetAllForListVM model)
        {
            try
            {
                if (model != null)
                {
                    if (model.Filters != null)
                    {
                        DocumentCCFiltersGetAllVM filtersToSend2 = new DocumentCCFiltersGetAllVM();

                        if (model.Filters.StartDate != null || model.Filters.EndDate != null)
                        {
                            if (model.Filters.StartDate != null && model.Filters.EndDate == null)
                            {
                                ViewData["StartDateFilled"] = "True";
                                ViewData["EndDateFilled"] = "False";
                                ViewData["DatePickerValidationMessage"] = "Selecciona la fecha hasta";
                                return View(model);
                            }
                            else if (model.Filters.StartDate == null && model.Filters.EndDate != null)
                            {
                                ViewData["StartDateFilled"] = "False";
                                ViewData["EndDateFilled"] = "True";
                                ViewData["DatePickerValidationMessage"] = "Selecciona la fecha desde";
                                return View(model);
                            }
                        }
                        if (model.Filters.StartDate != null && model.Filters.EndDate != null)
                        {
                            if (model.Filters.StartDate > model.Filters.EndDate)
                            {
                                ViewData["StartDateFilled"] = "IsGreater";
                                ViewData["DatePickerValidationMessage"] = "La fecha desde no puede ser mayor a la fecha hasta";
                                return View(model);
                            }
                        }
                    }
                }

                DocumentCCFiltersGetAllVM filtersToSend = new DocumentCCFiltersGetAllVM();
                Pagination paginationToSend = new Pagination();

                if (model.Pagination == null)
                {
                    paginationToSend = new Pagination();
                }
                else
                {
                    paginationToSend = model.Pagination;
                    if (model.Filters.SearchText != filtersToSend.SearchText)
                    {
                        paginationToSend.PageIndex = 1;
                    }
                }
                if (model.Filters == null)
                {
                    filtersToSend.CompanyId = null;
                }
                else
                {
                    filtersToSend = model.Filters;
                }
                DocumentCCGetAllForListVM modelToSend = new DocumentCCGetAllForListVM
                {
                    Pagination = paginationToSend,
                    Filters = filtersToSend

                };

                var documentTypes = (List<SelectVM>?) await _unitOfWork.DocumentCC.GetDocumentsTypeWhereDocumentsExistAsync();
                List<SelectVM> documentTypesList = new List<SelectVM>();
                if (documentTypes != null)
                {
                    foreach (var docType in documentTypes)
                    {
                        documentTypesList.Add(new SelectVM { Value = docType.Value, Name = docType.Name });
                    }
                }

                var clients = (await _unitOfWork.Client.GetAllAsync(x => x.ClientCode != "OCELL_C0001"
                && x.ClientCode != "OCE_C0028" && x.ClientCode != "OCE_C0029" && x.ClientCode != "OCE_C0030")).OrderBy(x => x.Name);
                List<SelectVM> clientList = new List<SelectVM>();
                if (clients != null)
                {
                    foreach (var client in clients)
                    {
                        clientList.Add(new SelectVM { Value = client.ClientId.ToString(), Name = client.Name });
                    }
                }
                var totalResults = await _unitOfWork.DocumentCC.GetAllDocumentsCCWithFiltersAsync(modelToSend);
                int totalNum = totalResults.totalCount;
                int totalPages = (int)Math.Ceiling(totalNum / (double)modelToSend.Pagination.PageSize);

                ViewData["TotalPages"] = totalPages;

                modelToSend.Pagination.PageIndex = Math.Max(1, Math.Min(modelToSend.Pagination.PageIndex, totalPages));
                modelToSend.Pagination.TotalResults = totalResults.totalCount;

                DocumentCCGetAllForListVM viewModel = new DocumentCCGetAllForListVM
                {
                    DocumentsCCList = totalResults.documentsCC,
                    Pagination = modelToSend.Pagination,
                    Filters = modelToSend.Filters,
                    ClientList = clientList,
                    DocumentTypeList = (List<SelectVM>)documentTypes
                };
                return View(viewModel);
            }
            catch (Exception ex)
            {
                return RedirectToAction("Error", "Home", new { area = "" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetInvoicesWithDaysExpired()
        {
            try
            {
                var expiredDocs = await _unitOfWork.DocumentCC.GetAllExpiredDocsWithDaysExpiredFiltersAsync();
                return Json(expiredDocs);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    errors = new[] { $"Hubo un error extrayendo la lista de documentos." },
                    result = "errorGet",
                    detail = ex.Message
                });
            }
        }
        [HttpPost]
        public async Task<IActionResult> SendCXCStatus()
        {
            try
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                var emailClaim = claimsIdentity.FindFirst(ClaimTypes.Email);
                var emailSlackSender = emailClaim?.Value;
                string slackSenderId = "<@"; try
                {
                    slackSenderId = slackSenderId + await _slackRepository.Value.GetSlackUserIdByEmailAsync(emailSlackSender) + ">";
                }
                catch (Exception ex)
                {
                    slackSenderId = emailSlackSender;
                }
                string slackChannelId = _config["Slack:SuccessManagerChannel"];
                var notificationStatus = await _unitOfWork.NotificationStatus.GetFirstOrDefaultAsync(x => x.Name == "Enviado");
                if (notificationStatus == null)
                {
                    return Json(new { success = false, error = "Error en la obtención de datos." });
                }
                var pendingInvoices = await _unitOfWork.DocumentCC.GetAllExpiredPendingDocsAsync();
                pendingInvoices = pendingInvoices.OrderBy(doc => doc.ClientName).ThenByDescending(doc => doc.NumDaysExpired).ToList();
                var client = "";
                var invoiceLine = "";
                foreach (var invoice in pendingInvoices)
                {
                    var invoiceIcon = "";
                    var clientIcon = "";
                    if (invoice.NumDaysExpired >= 1 && invoice.NumDaysExpired < 5)
                    {
                        invoiceIcon = ":upside_down_face:";
                        clientIcon = ":information_source:";
                    }
                    else if (invoice.NumDaysExpired >= 5 && invoice.NumDaysExpired < 15)
                    {
                        invoiceIcon = ":thinking_face:";
                        clientIcon = ":warning:";
                    }
                    else
                    {
                        invoiceIcon = ":exploding_head:";
                        clientIcon = ":alert:";
                    }
                    if (client != invoice.ClientName)
                    {
                        string slackSuccessManagerId = "No Success Manager is assigned yet.";
                        if (invoice.SuccessManagerEmail != null)
                        {
                            var successManagerUser = await _unitOfWork.ApplicationUser.GetFirstOrDefaultAsync(x => x.Email == invoice.SuccessManagerEmail);
                            if (successManagerUser == null)
                            {
                                return Json(new { success = false, error = "No fue posible encontrar el usuario para el Success Manager en la base de datos." });
                            }
                            try
                            {
                                slackSuccessManagerId = "<@" + await _slackRepository.Value.GetSlackUserIdByEmailAsync(successManagerUser.Email) + @">";
                            }
                            catch (Exception ex)
                            {
                                slackSuccessManagerId = "The user " + successManagerUser.Email + " assigned as Success Manager is not a user from Slack or is not an active user.";
                            }

                        }
                        invoiceLine = invoiceLine + "\n *" + invoice.ClientName.ToUpper() + ":* " + clientIcon + "\n" +
                        "*Success Manager:* " + slackSuccessManagerId + "\n" +
                                "• " + invoiceIcon + " *Invoice #:* " + invoice.DocumentNumber + ", *Date:* " + invoice.DocumentDate.ToString("MM/dd/yyyy") + ", *Exp Date:* " + invoice.ExpirationDate.ToString("MM/dd/yyyy") + ", *Days Expired:* " + invoice.NumDaysExpired + ", *# Notifications sent:* " + invoice.NumNotificationsSent + ".\n";
                    }
                    else
                    {
                        invoiceLine = invoiceLine + "• " + invoiceIcon + " *Invoice #:* " + invoice.DocumentNumber + ", *Date:* " + invoice.DocumentDate.ToString("MM/dd/yyyy") + ", *Exp Date:* " + invoice.ExpirationDate.ToString("MM/dd/yyyy") + ", *Days Expired:* " + invoice.NumDaysExpired + ", *# Notifications sent:* " + invoice.NumNotificationsSent + ".\n";
                    }
                    client = invoice.ClientName;
                }
                var slackBody = ":mega: *ACCOUNTS RECEIVABLE STATUS FROM CLIENTS* :mega: \n\n" +
                "Hi Team!, \n" +
                slackSenderId + " is sending you the status of the clients, see the info below: \n" +
                invoiceLine + "\n Everyone else is up to date. :white_check_mark:";
                try
                {
                    await _slackRepository.Value.SendMessageToChannelAsync(slackChannelId, slackBody);
                }
                catch (Exception ex)
                {
                    notificationStatus = await _unitOfWork.NotificationStatus.GetFirstOrDefaultAsync(x => x.Name == "Envío fallido");
                }
                var recipient = new SaveNotificationRecipientVM()
                {
                    NotificationMedia = "Slack",
                    RecipientMediaInfo = "Slack Channel Id: " + slackChannelId
                };
                var recipients = new List<SaveNotificationRecipientVM>();
                recipients.Add(recipient);
                try
                {
                    _unitOfWork.Notification.SaveNotification("Cuentas por cobrar", "ACCOUNTS RECEIVABLE STATUS FROM CLIENTS",
                   slackBody, "Accounting App", claim.Value, recipients, notificationStatus.NotificationStatusId);
                }
                catch (Exception ex)
                {
                    return BadRequest(new
                    {
                        error = $"Hubo un error guardando la notificación en la base de datos, el estado del mensaje enviado a Slack es: " + notificationStatus + ", reporta este issue al administrador.",
                        result = "errorPost",
                        detail = ex.Message
                    });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    error = $"Hubo un error en la solicitud, intentalo más tarde o reporta este issue al administrador.",
                    result = "errorPost",
                    detail = ex.Message
                });
            }
            return Json(new
            {
                success = true,
                message = "Le acabas de enviar el status de CXC al canal de success-management"
            });
        }

        [HttpPost]
        public async Task<IActionResult> SendNotification(int documentId)
        {
            using var transaction = await _unitOfWork.BeginTranAsync();
            try
            {
                var documentCC = await _unitOfWork.DocumentCC.GetFirstOrDefaultAsync(x => x.DocumentCCId == documentId);
                var client = documentCC != null ? await _unitOfWork.Client.GetFirstOrDefaultAsync(x => x.ClientId == documentCC.ClientId) : null;
                var notificationType = await _unitOfWork.NotificationType.GetFirstOrDefaultAsync(x => x.Name == "Cuentas por cobrar");
                var notificationMediaEmail = await _unitOfWork.NotificationMedia.GetFirstOrDefaultAsync(x => x.Name == "Email");
                var notificationMediaSlack = await _unitOfWork.NotificationMedia.GetFirstOrDefaultAsync(x => x.Name == "Slack");
                var slackChannelId = _config["Slack:SuccessManagerChannel"];
                var returnSuccessMessage = "¡Bien, le acabas de enviar un recordatorio de pago a: " + client.Name + " por email y una notifiación a los Success Managers al canal de Slack!";

                if (documentCC == null || client == null || notificationType == null || notificationMediaEmail == null
                    || notificationMediaSlack == null)
                {
                    return Json(new { success = false, error = "Error en la obtención de datos." });
                }

                string documentMonth = documentCC.DocumentDate.ToString("MMMM");
                DateTime docExpirationDate = documentCC.DocumentDate.AddDays(double.Parse(client.PaymentCondition));
                var subjectEmail = $"Invoice from {documentMonth} is still pending payment.";
                var subjectSlack = "";
                var emailsSentToForSlackNot = "";

                var allEmails = client.Emails.Split(new[] { ';', ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                List<string> additionalEmailsForNotifications = new List<string>();
                additionalEmailsForNotifications = client.AdditionalEmailsForNotifications?.Split(new[] { ';', ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList() ?? additionalEmailsForNotifications;

                var emailTo = allEmails.FirstOrDefault();
                var emailsCCBeforeRemoveDuplicates = allEmails.Skip(1).ToList();
                string slackSuccessManagerId = "No Success Manager is assigned yet.";

                foreach (var email in additionalEmailsForNotifications)
                {
                    emailsCCBeforeRemoveDuplicates.Add(email);
                }
                if (client.SuccessManager != null)
                {
                    var consultantDetail = await _unitOfWork.ConsultantDetail.GetFirstOrDefaultAsync(x => x.ConsultantId == client.SuccessManager);
                    var successManagerUser = await _unitOfWork.ApplicationUser.GetFirstOrDefaultAsync(x => x.Id == consultantDetail.UserId);
                    if (successManagerUser == null)
                    {
                        return Json(new { success = false, error = "No fue posible encontrar el usuario para el Success Manager en la base de datos." });
                    }
                    emailsCCBeforeRemoveDuplicates.Add(successManagerUser.Email);
                    try
                    {
                        slackSuccessManagerId = "<@" + await _slackRepository.Value.GetSlackUserIdByEmailAsync(successManagerUser.Email) + @">";
                    }
                    catch (Exception ex)
                    {
                        slackSuccessManagerId = "The user " + successManagerUser.Email + " assigned as Success Manager is not a user from Slack.";
                    }

                }

                List<string> emailsCC = new List<string>();

                foreach (var email in emailsCCBeforeRemoveDuplicates)
                {
                    if (!emailsCC.Contains(email.Trim()))
                    {
                        emailsCC.Add(email.Trim());
                    }
                }

                var emailsCCString = "";
                foreach (var email in emailsCC)
                {
                    emailsCCString = emailsCCString + @"• " + email + "\n";
                }
                var costaRicaTime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, "Central America Standard Time");
                int numDaysExpired = (int)(costaRicaTime - documentCC.DocumentDate).TotalDays - int.Parse(client.PaymentCondition);
                var emailBody = "";
                var slackBody = "";

                if (string.IsNullOrEmpty(emailTo))
                {
                    return Json(new { success = false, error = "El cliente no tiene correos electrónicos." });
                }
                var alreadyNotificationSent = await _unitOfWork.DocumentsCCNotification.GetAllAsync(x => x.DocumentCCId == documentId);

                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                var emailClaim = claimsIdentity.FindFirst(ClaimTypes.Email);
                var emailSlackSender = emailClaim?.Value;

                string slackSenderId = "<@"; try
                {
                    slackSenderId = slackSenderId + await _slackRepository.Value.GetSlackUserIdByEmailAsync(emailSlackSender) + ">";
                }
                catch (Exception ex)
                {
                    slackSenderId = emailSlackSender;
                }

                var notificationEmail = new Notification();
                notificationEmail.NotificationTypeId = notificationType.NotificationTypeId;
                notificationEmail.Subject = subjectEmail;
                notificationEmail.Remitent = Environment.GetEnvironmentVariable(_config["internalEmail"]);
                notificationEmail.SentDate = costaRicaTime;
                notificationEmail.SentByUser = claim.Value;

                var notificationSlack = new Notification();
                notificationSlack.NotificationTypeId = notificationType.NotificationTypeId;
                notificationSlack.Remitent = _config["Slack:AccountingAppName"];
                notificationSlack.SentDate = costaRicaTime;
                notificationSlack.SentByUser = claim.Value;

                var notificationStatus = await _unitOfWork.NotificationStatus.GetFirstOrDefaultAsync(x => x.Name == "Enviado");
                if (notificationStatus == null)
                {
                    return Json(new { success = false, error = "Error en la obtención de datos." });
                }
                var additionalSubIntro = "";
                var actionsToTake = "";

                var lateFee = "";
                var totalAmountDue = (documentCC.BalanceAmount).ToString("#,##0.00");
                if (numDaysExpired >= 30 && client.LatePaymentFee > 0)
                {
                    lateFee = "• *Late Fee Amount:* $" + (documentCC.BalanceAmount * client.LatePaymentFee).ToString("#,##0.00") + " \n";
                    totalAmountDue = (documentCC.BalanceAmount + (documentCC.BalanceAmount * client.LatePaymentFee)).ToString("#,##0.00");
                }
                var invoiceDetails = "• *Invoice Number:* " + documentCC.DocumentNumber + " \n" +
         "• *Total Amount:* $" + documentCC.DocumentAmount.ToString("#,##0.00") + " \n" +
         lateFee +
         "• *Total Amount Due:* $" + totalAmountDue + " \n" +
         "• *Date:* " + documentCC.DocumentDate.ToString("MM/dd/yyyy") + " \n" +
         "• *Expiration Date:* " + docExpirationDate.ToString("MM/dd/yyyy") + " \n" +
         "• *Days Expired:* " + numDaysExpired;

                emailsSentToForSlackNot = "The payment reminder email was sent to " + emailTo + ", with copy to the following emails: \n" +
emailsCCString;

                if (numDaysExpired >= 5 && numDaysExpired < 15 && alreadyNotificationSent.Count() == 0)
                {
                    subjectSlack = $"INVOICE FROM {client.Name.ToUpper()} IS STILL PENDING PAYMENT* :warning:";
                    additionalSubIntro = slackSenderId + " has sent a payment reminder to *" + client.Name.ToUpper() + "*, ";
                    actionsToTake = "• If client is known and a consistent payer, wait more days, at the discretion of Accounts Receivable.";
                    emailBody = emailBodyYellow(
                            client.Name,
                            documentCC.BalanceAmount,
                            documentCC.DocumentAmount,
                            documentMonth,
                            documentCC.DocumentDate.Year,
                            documentCC.DocumentNumber,
                            documentCC.DocumentDate,
                            docExpirationDate,
                            numDaysExpired);
                }
                else if (numDaysExpired >= 15 && numDaysExpired < 30 && alreadyNotificationSent.Count() == 1)
                {
                    subjectSlack = $"INVOICE FROM {client.Name.ToUpper()} IS STILL PENDING PAYMENT* :alert:";
                    actionsToTake = "• Success manager should send additional message to main point of contact on the client side." + " \n" +
                        "• If client is known and a consistent payer, wait more days, at the discretion of Accounts Receivable, wait for Success Manager to engage client with the inquiry.";
                    additionalSubIntro = slackSenderId + " has sent a payment reminder to *" + client.Name.ToUpper() + "*, ";
                    if (client.LatePaymentFee > 0)
                    {
                        additionalSubIntro = slackSenderId + " has sent a payment reminder to *" + client.Name.ToUpper() + "*, including the *late fee notice of " + (client.LatePaymentFee * 100).ToString("#,##0") + "%*, ";
                    }

                    emailBody = emailBodyRed(
                            client.Name,
                            documentCC.BalanceAmount,
                            documentCC.DocumentAmount,
                            documentMonth,
                            documentCC.DocumentDate.Year,
                            documentCC.DocumentNumber,
                            documentCC.DocumentDate,
                            docExpirationDate,
                            numDaysExpired, client.LatePaymentFee);
                }
                else if (numDaysExpired >= 30 && numDaysExpired < 45 && alreadyNotificationSent.Count() == 2)
                {
                    subjectSlack = $"INVOICE FROM {client.Name.ToUpper()} IS STILL PENDING PAYMENT* :alert:";
                    actionsToTake = "• Phone call to the client from Success Manager. If possible, escalate to highest known management contact on the client side." + " \n" +
                        "• Slack conversations with client that evidence that the services were provided successfully." + " \n" +
                        "• Emails showing the client’s intention to pay and no objection about the hours reported.";
                    additionalSubIntro = slackSenderId + " has sent a payment reminder to *" + client.Name.ToUpper() + "*, ";
                    if (client.LatePaymentFee > 0)
                    {
                        additionalSubIntro = slackSenderId + " has sent a payment reminder to *" + client.Name.ToUpper() + "*, including the *late fee of " + (client.LatePaymentFee * 100).ToString("#,##0") + "%*, ";
                    }

                    emailBody = emailBodyRed30Days(
                            client.Name,
                            documentCC.BalanceAmount,
                            documentCC.DocumentAmount,
                            documentMonth,
                            documentCC.DocumentDate.Year,
                            documentCC.DocumentNumber,
                            documentCC.DocumentDate,
                            docExpirationDate,
                            numDaysExpired, client.LatePaymentFee);
                }
                else if (numDaysExpired >= 45 && numDaysExpired < 60 && alreadyNotificationSent.Count() == 3)
                {
                    subjectSlack = $"INVOICE FROM {client.Name.ToUpper()} IS STILL PENDING PAYMENT* :alert:";
                    actionsToTake = "• Request meeting with client." + " \n" +
                        "• Discuss with client about potential payment arrangements." + " \n" +
                        "• If client is not engaging live or has no time to connect, send via email notification instead.";
                    additionalSubIntro = slackSenderId + " is sending you a reminder to let you know that payment from *" + client.Name +
                        "* is still pending *" + numDaysExpired + " days late*";
                    emailsSentToForSlackNot = "No emails were sent to the client regarding this notification, this is just an internal reminder.";

                }
                else if (numDaysExpired >= 60 && numDaysExpired < 75 && alreadyNotificationSent.Count() == 4)
                {
                    subjectSlack = $"INVOICE FROM {client.Name.ToUpper()} IS STILL PENDING PAYMENT* :alert:";
                    actionsToTake = "• Continue engagement with client about possible payment arrangements." + " \n" +
                        "• Final live notice with potential consequences (15-day notice to remove consultant)." + " \n" +
                        "• Notify the consultant of the situation and potential finalization in the project if no payment arrangement is achieve." + " \n" +
                        "• If client is not engaging live or has no time to connect, send formal notification of potential consequences via written form.";
                    additionalSubIntro = slackSenderId + " is sending you a reminder to let you know that payment from *" + client.Name +
                        "* is still pending *" + numDaysExpired + " days late*";
                    emailsSentToForSlackNot = "No emails were sent to the client regarding this notification, this is just an internal reminder.";
                }
                else if (numDaysExpired >= 75 && numDaysExpired < 90 && alreadyNotificationSent.Count() == 5)
                {
                    subjectSlack = $"INVOICE FROM {client.Name.ToUpper()} IS STILL PENDING PAYMENT* :alert:";
                    actionsToTake = "• If still unpaid, send notice of finalization from the consultant’s services." + " \n" +
                        "• Notify consultant and proceed with finalization of the contract.";
                    additionalSubIntro = slackSenderId + " is sending you a reminder to let you know that payment from *" + client.Name +
                        "* is still pending *" + numDaysExpired + " days late*";
                    emailsSentToForSlackNot = "No emails were sent to the client regarding this notification, this is just an internal reminder.";
                }
                else if (numDaysExpired >= 90 && numDaysExpired < 120 && alreadyNotificationSent.Count() == 6)
                {
                    subjectSlack = $"INVOICE FROM {client.Name.ToUpper()} IS STILL PENDING PAYMENT* :alert:";
                    actionsToTake = "• Send formal notice to client of intent to pursue other methods of collection.";
                    additionalSubIntro = slackSenderId + " is sending you a reminder to let you know that payment from *" + client.Name +
                        "* is still pending *" + numDaysExpired + " days late*";
                    emailsSentToForSlackNot = "No emails were sent to the client regarding this notification, this is just an internal reminder.";
                }
                else if (numDaysExpired >= 120 && alreadyNotificationSent.Count() == 7)
                {
                    subjectSlack = $"INVOICE FROM {client.Name.ToUpper()} IS STILL PENDING PAYMENT* :alert:";
                    actionsToTake = "• If client is not engaging or unwilling to pay." + " \n" +
                        "• Escalation to collection agency." + " \n" +
                        "• Currently using GGR as our preferred collection agency. Contact: aesquibel@ggrinc.com.";
                    additionalSubIntro = slackSenderId + " is sending you a reminder to let you know that payment from *" + client.Name +
                        "* is still pending *" + numDaysExpired + " days late*";
                    emailsSentToForSlackNot = "No emails were sent to the client regarding this notification, this is just an internal reminder.";
                }
                slackBody = SlackBodyNotification(subjectSlack,
                            invoiceDetails, additionalSubIntro,
                            emailsSentToForSlackNot, actionsToTake, slackSuccessManagerId);
                //IF email should be sent
                if (alreadyNotificationSent.Count() >= 0 && alreadyNotificationSent.Count() <= 2)
                { //Add emails as CC

                    notificationEmail.Body = emailBody;

                    await _unitOfWork.Notification.AddAsync(notificationEmail);
                    await _unitOfWork.SaveAsync();

                    emailsCC.Add(emailTo); //Add emailTo to

                    List<string> emailsCCAfterRemoveDuplicates = new List<string>();

                    foreach (var email in emailsCC)
                    {
                        if (!emailsCCAfterRemoveDuplicates.Contains(email.Trim()))
                        {
                            emailsCCAfterRemoveDuplicates.Add(email.Trim());
                        }
                    }

                    var emailToSend = new SendEmailVM();
                    emailToSend.Subject = subjectEmail;
                    emailToSend.EmailTo = emailTo;
                    emailToSend.SharedEmailFrom = Environment.GetEnvironmentVariable(_config["internalEmail"]);
                    emailToSend.EmailCcList = emailsCCAfterRemoveDuplicates;
                    emailToSend.Body = emailBody;

                    try
                    {
                        var emailSent = _sendEmail.Value.SendEmail(emailToSend);
                        var documentNotification = new DocumentsCCNotification()
                        {
                            DocumentCCId = documentId,
                            NotificationId = notificationEmail.NotificationId
                        };
                        await _unitOfWork.DocumentsCCNotification.AddAsync(documentNotification);
                        await _unitOfWork.SaveAsync();
                    }
                    catch (Exception ex)
                    {
                        notificationStatus = await _unitOfWork.NotificationStatus.GetFirstOrDefaultAsync(x => x.Name == "Envío fallido");
                    }
                    foreach (var email in emailsCCAfterRemoveDuplicates)
                    {
                        var recipientUserCC = await _unitOfWork.ApplicationUser.GetFirstOrDefaultAsync(x => x.Email == email);
                        var recipientUserIdCC = recipientUserCC?.Id;
                        var notificationRecipientCC = new NotificationRecipient()
                        {
                            RecipientMediaInfo = email,
                            NotificationId = notificationEmail.NotificationId,
                            NotificationMediaId = notificationMediaEmail.NotificationMediaId,
                            NotificationStatusId = notificationStatus.NotificationStatusId,
                            RecipientUserId = recipientUserIdCC
                        };
                        await _unitOfWork.NotificationRecipient.AddAsync(notificationRecipientCC);
                    }
                    await _unitOfWork.SaveAsync();
                }
                //Save Slack notification
                notificationSlack.Body = slackBody;
                notificationSlack.Subject = subjectSlack;

                await _unitOfWork.Notification.AddAsync(notificationSlack);
                await _unitOfWork.SaveAsync();

                notificationStatus = await _unitOfWork.NotificationStatus.GetFirstOrDefaultAsync(x => x.Name == "Enviado");
                if (notificationStatus == null)
                {
                    return Json(new { success = false, error = "Error en la obtención de datos." });
                }

                try
                {
                    await _slackRepository.Value.SendMessageToChannelAsync(slackChannelId, slackBody);
                }
                catch (Exception ex)
                {
                    notificationStatus = await _unitOfWork.NotificationStatus.GetFirstOrDefaultAsync(x => x.Name == "Envío fallido");
                }
                if (emailBody == "")
                {
                    returnSuccessMessage = "¡Bien, acabas de enviar una notificación a los success managers en el canal de Slack!";
                    var documentNotification = new DocumentsCCNotification()
                    {
                        DocumentCCId = documentId,
                        NotificationId = notificationSlack.NotificationId
                    };
                    await _unitOfWork.DocumentsCCNotification.AddAsync(documentNotification);
                    await _unitOfWork.SaveAsync();
                }
                var notificationRecipientSlack = new NotificationRecipient()
                {
                    RecipientMediaInfo = "Slack Channel Id: " + slackChannelId,
                    NotificationId = notificationSlack.NotificationId,
                    NotificationMediaId = notificationMediaSlack.NotificationMediaId,
                    NotificationStatusId = notificationStatus.NotificationStatusId,
                    RecipientUserId = null
                };
                await _unitOfWork.NotificationRecipient.AddAsync(notificationRecipientSlack);
                await _unitOfWork.SaveAsync();

                await transaction.CommitAsync();
                return Json(new { success = true, message = returnSuccessMessage });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    error = $"Hubo un error en la solicitud, intentalo más tarde o reporta este issue al administrador.",
                    result = "errorPost",
                    detail = ex.Message
                });
            }
        }

        [HttpPost]
        public IActionResult GetNotificationsHistoryByDocument(int documentId)
        {
            try
            {
                var notificationsHistory = _unitOfWork.DocumentCC.GetNotificationsHistoryByDocumentIdAsync(documentId);
                return Json(new { success = true, message = $"Bien!", notificationHistory = notificationsHistory });
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        private string SlackBodyNotification(string subject, string invoiceDetails,
          string additionalSubIntro,
            string emailsSentTo, string actionsToTake, string successManagerSlackId)
        {
            var body = "*" + subject + "\n\n" +
           "Hi Team!, :smiley: \n\n" +
            additionalSubIntro +
           " regarding the invoice below: \n" +
           invoiceDetails + "\n\n" +
           "*SUCCESS MANAGER* :necktie:" + "\n" +
           successManagerSlackId + "\n\n" +
           "*ACTIONS TO TAKE* :male-detective::skin-tone-3:" + "\n" +
           actionsToTake + "\n\n" +
           "*EMAILS SENT* :incoming_envelope:" + "\n" +
           emailsSentTo;

            return body;
        }
        private string emailBodyYellow(string clientName, decimal totalAmountDue, decimal documentAmount,
            string month,
            int year,
            string documentNumber,
            DateTime documentDate,
            DateTime docExpirationDate, int numDaysExpired)
        {
            var body = @"
<!DOCTYPE html>
<html>

<head>
</head>

<body>
    <table cellspacing=""0"" cellpadding=""0"" width=""100%"">
        <tr>
            <td>
                <table cellspacing=""0"" cellpadding=""0"" width=""500""
                style=""background-color: #fff; border: #000 solid 2px; padding: 10px;"">
                <tr>
                    <td>
                        <table cellspacing=""0"" cellpadding=""0"" width=""500"">
                            <tr>
                                <td align=""left"">
                                    <table cellspacing=""0"" cellpadding=""0"" width=""500""
                                        style=""background-color: #f5f6f7; margin: 0 auto; position: relative;z-index: 1; border-top-left-radius: 10px; border-top-right-radius: 10px;"">
                                        <tr>
                                            <td>
                                                <img src=""https://oceansapp.azurewebsites.net/img/email/email-header.png""
                                                    alt=""Oceans Code Experts"" width=""100%"" />
                                            </td>
                                        </tr>
                                    </table>
                                    <table cellspacing=""0"" cellpadding=""0"" width=""500""
                                        style=""background-color: #f5f6f7; margin: 0 auto; border-bottom-left-radius: 10px; border-bottom-right-radius: 10px; position: relative; padding: 0 30px 30px 30px"">
                                        <tr>
                                            <td>
                                                <div>
                                                    <div>
                                                        <table cellspacing=""0"" cellpadding=""0"" width=""100%""
                                                            style=""font-family: 'nexa', Arial, sans-serif;"">
                                                            <tr>
                                                                <td>
                                                                    <table cellspacing=""0"" cellpadding=""0"" width=""100%"">
                                                                        <tr>
                                                                            <td>
                                                                                <hr style=""margin:0"" />
                                                                                <h2 style=""text-align: left; margin-bottom: 0;"">
                                                                                    OVERDUE
                                                                                    INVOICE PAYMENT</h2>
                                                                            </td>
                                                                        </tr>
                                                                    </table>
                                                                    <p>Dear " + clientName + @",</p>
                                                                    <p>Your attention is needed regarding an overdue balance of
                                                                        <strong>$" + totalAmountDue.ToString("#,##0.00") + @"</strong> for services
                                                                        delivered in the
                                                                        period of <strong>" + month + @" " + year + @"</strong>.
                                                                    </p>
                                                                    <table cellspacing=""0"" cellpadding=""0"" width=""100%"">
                                                                        <tr>
                                                                            <td
                                                                                style=""border-left: 8px solid #eeb30f; padding-left: 10px; margin-top: 20px;"">
                                                                                <p
                                                                                    style=""color: #eeb30f; margin: 0 0 10px 0; font-size: 18px;"">
                                                                                    <strong> INVOICE
                                                                                        NUMBER:</strong> " + documentNumber + @"
                                                                                </p>
                                                                                <table cellspacing=""0"" cellpadding=""0""
                                                                                    style=""font-size: 14px;"">
                                                                                    <tr>
                                                                                        <td
                                                                                            style=""border-right: 2px solid #9ba8b8; padding-right: 10px;"">
                                                                                            <strong> TOTAL AMOUNT</strong>
                                                                                        </td>
                                                                                        <td
                                                                                            style=""border-left: 2px solid #9ba8b8; padding-left: 10px;"">
                                                                                            $" + documentAmount.ToString("#,##0.00") + @"
                                                                                        </td>
                                                                                    </tr>
                                                                                    <tr>
                                                                                        <td
                                                                                            style=""border-right: 2px solid #9ba8b8; padding-right: 10px;"">
                                                                                            <strong> TOTAL AMOUNT DUE</strong>
                                                                                        </td>
                                                                                        <td
                                                                                            style=""border-left: 2px solid #9ba8b8; padding-left: 10px;"">
                                                                                            $" + totalAmountDue.ToString("#,##0.00") + @"
                                                                                        </td>
                                                                                    </tr>
                                                                                    <tr>
                                                                                        <td
                                                                                            style=""border-right: 2px solid #9ba8b8; padding-right: 10px;"">
                                                                                            <strong>DATE</strong>
                                                                                        </td>
                                                                                        <td
                                                                                            style=""border-left: 2px solid #9ba8b8; padding-left: 10px;"">
                                                                                            " + documentDate.ToString("MM/dd/yyyy") + @"
                                                                                        </td>
                                                                                    </tr>
                                                                                    <tr>
                                                                                        <td
                                                                                            style=""border-right: 2px solid #9ba8b8; padding-right: 10px;"">
                                                                                            <strong> EXPIRATION DATE</strong>
                                                                                        </td>
                                                                                        <td
                                                                                            style=""border-left: 2px solid #9ba8b8; padding-left: 10px;"">
                                                                                            <span
                                                                                                style=""color: red;"">" + docExpirationDate.ToString("MM/dd/yyyy") + @"</span>
                                                                                        </td>
                                                                                    </tr>
                                                                                    <tr>
                                                                                        <td
                                                                                            style=""border-right: 2px solid #9ba8b8; padding-right: 10px;"">
                                                                                            <strong> DAYS EXPIRED</strong>
                                                                                        </td>
                                                                                        <td
                                                                                            style=""border-left: 2px solid #9ba8b8; padding-left: 10px;"">
                                                                                            <span
                                                                                                style=""color: red;"">" + numDaysExpired + @"</span>
                                                                                        </td>
                                                                                    </tr>
                                                                                </table>
                                                                            </td>
                                                                        </tr>
                                                                    </table>
                                                                    <p> Please return confirmation upon receipt of this message
                                                                        and let us
                                                                        know if you need further clarification or assistance by
                                                                        replying to
                                                                        this email or at <a
                                                                            href=""mailto:oscar.alfaro@oceanscode.com"">oscar.alfaro@oceanscode.com</a>.
                                                                        Your prompt response is appreciated.</p>
                                                                    <table cellspacing=""0"" cellpadding=""0"" width=""100%"">
                                                                        <tr>
                                                                            <td style=""text-align: left;"">
                                                                                <p style=""margin-top: 8px; margin-bottom: 5px;"">
                                                                                    Thank you,
                                                                                </p>
                                                                                <p style=""margin-top: 2px;""> Oscar Alfaro,
                                                                                    Finance Manager.
                                                                                </p>
                                                                            </td>
                                                                        </tr>
                                                                    </table>
                                                                    <table cellspacing=""0"" cellpadding=""0"" width=""100%"">
                                                                        <tr>
                                                                            <td style=""text-align: left;"">
                                                                                <img src=""https://oceansapp.azurewebsites.net/img/accounting-firm.png""
                                                                                    width=""100%"" alt=""Accounting"" />
                                                                            </td>
                                                                        </tr>
                                                                    </table>
                                                                    <table cellspacing=""0"" cellpadding=""0"" width=""100%"">
                                                                        <tr>
                                                                            <td style=""text-align: left;"">
                                                                                <p style=""margin-top: 8px; margin-bottom: 5px;"">
                                                                                   <strong>This is an automated email, ignore if you are up to date with your payments.</strong>
                                                                                </p>
                                                                            </td>
                                                                        </tr>
                                                                    </table>
                                                                </td>
                                                            </tr>
                                                        </table>
                                                    </div>
                                                </div>
                                            </td>
                                        </tr>
                                    </table>
                                </td>
                            </tr>
                        </table>
                    </td>
                </tr>
            </table>
            </td>
        </tr>
    </table>
</body>

</html>";
            return body;
        }

        private string emailBodyRed(string clientName, decimal totalAmountDue, decimal documentAmount,
            string month,
            int year,
            string documentNumber,
            DateTime documentDate,
            DateTime docExpirationDate, int numDaysExpired, decimal latePaymentFee)
        {
            var note = "";
            if (latePaymentFee > 0)
            {
                note = @"<p><strong>Note that a late payment fee of " + (latePaymentFee * 100).ToString("#,##0") + @"% of the invoice will be included if you do not pay within the next 15 days.</strong>
                        </p>";
            }
            var body = @"
<!DOCTYPE html>
<html>

<head>
</head>

<body>
    <table cellspacing=""0"" cellpadding=""0"" width=""100%"">
        <tr>
            <td>
                <table cellspacing=""0"" cellpadding=""0"" width=""500""
                style=""background-color: #fff; border: #000 solid 2px; padding: 10px;"">
                <tr>
                    <td>
                        <table cellspacing=""0"" cellpadding=""0"" width=""500"">
                            <tr>
                                <td align=""left"">
                                    <table cellspacing=""0"" cellpadding=""0"" width=""500""
                                        style=""background-color: #f5f6f7; margin: 0 auto; position: relative;z-index: 1; border-top-left-radius: 10px; border-top-right-radius: 10px;"">
                                        <tr>
                                            <td>
                                                <img src=""https://oceansapp.azurewebsites.net/img/email/email-header-red.png""
                                                    alt=""Oceans Code Experts"" width=""100%"" />
                                            </td>
                                        </tr>
                                    </table>
                                    <table cellspacing=""0"" cellpadding=""0"" width=""500""
                                        style=""background-color: #f5f6f7; margin: 0 auto; border-bottom-left-radius: 10px; border-bottom-right-radius: 10px; position: relative; padding: 0 30px 30px 30px"">
                                        <tr>
                                            <td>
                                                <div>
                                                    <div>
                                                        <table cellspacing=""0"" cellpadding=""0"" width=""100%""
                                                            style=""font-family: 'nexa', Arial, sans-serif;"">
                                                            <tr>
                                                                <td>
                                                                    <table cellspacing=""0"" cellpadding=""0"" width=""100%"">
                                                                        <tr>
                                                                            <td>
                                                                                <hr style=""margin:0"" />
                                                                                <h2 style=""text-align: left; margin-bottom: 0;"">OVERDUE PAYMENT REQUIRED</h2>
                                                                            </td>
                                                                        </tr>
                                                                    </table>
                                                                    <p>Dear " + clientName + @",</p>
                                                                    <p>Your immediate attention is needed regarding an overdue balance of 
                                                                        <strong>$" + totalAmountDue.ToString("#,##0.00") + @"</strong> for services
                                                                        delivered in the
                                                                        period of <strong>" + month + @" " + year + @"</strong>.
                                                                    </p>
                                                                   " + note + @"
                                                                    <table cellspacing=""0"" cellpadding=""0"" width=""100%"">
                                                                        <tr>
                                                                            <td
                                                                                style=""border-left: 8px solid red; padding-left: 10px; margin-top: 20px;"">
                                                                                <p
                                                                                    style=""color: red; margin: 0 0 10px 0; font-size: 18px;"">
                                                                                    <strong> INVOICE
                                                                                        NUMBER:</strong> " + documentNumber + @"
                                                                                </p>
                                                                                <table cellspacing=""0"" cellpadding=""0""
                                                                                    style=""font-size: 14px;"">
                                                                                    <tr>
                                                                                        <td
                                                                                            style=""border-right: 2px solid #9ba8b8; padding-right: 10px;"">
                                                                                            <strong> TOTAL AMOUNT</strong>
                                                                                        </td>
                                                                                        <td
                                                                                            style=""border-left: 2px solid #9ba8b8; padding-left: 10px;"">
                                                                                            $" + documentAmount.ToString("#,##0.00") + @"
                                                                                        </td>
                                                                                    </tr>
                                                                                    <tr>
                                                                                        <td
                                                                                            style=""border-right: 2px solid #9ba8b8; padding-right: 10px;"">
                                                                                            <strong> TOTAL AMOUNT DUE</strong>
                                                                                        </td>
                                                                                        <td
                                                                                            style=""border-left: 2px solid #9ba8b8; padding-left: 10px;"">
                                                                                            $" + totalAmountDue.ToString("#,##0.00") + @"
                                                                                        </td>
                                                                                    </tr>
                                                                                    <tr>
                                                                                        <td
                                                                                            style=""border-right: 2px solid #9ba8b8; padding-right: 10px;"">
                                                                                            <strong>DATE</strong>
                                                                                        </td>
                                                                                        <td
                                                                                            style=""border-left: 2px solid #9ba8b8; padding-left: 10px;"">
                                                                                            " + documentDate.ToString("MM/dd/yyyy") + @"
                                                                                        </td>
                                                                                    </tr>
                                                                                    <tr>
                                                                                        <td
                                                                                            style=""border-right: 2px solid #9ba8b8; padding-right: 10px;"">
                                                                                            <strong> EXPIRATION DATE</strong>
                                                                                        </td>
                                                                                        <td
                                                                                            style=""border-left: 2px solid #9ba8b8; padding-left: 10px;"">
                                                                                            <span
                                                                                                style=""color: red;"">" + docExpirationDate.ToString("MM/dd/yyyy") + @"</span>
                                                                                        </td>
                                                                                    </tr>
                                                                                    <tr>
                                                                                        <td
                                                                                            style=""border-right: 2px solid #9ba8b8; padding-right: 10px;"">
                                                                                            <strong> DAYS EXPIRED</strong>
                                                                                        </td>
                                                                                        <td
                                                                                            style=""border-left: 2px solid #9ba8b8; padding-left: 10px;"">
                                                                                            <span
                                                                                                style=""color: red;"">" + numDaysExpired + @"</span>
                                                                                        </td>
                                                                                    </tr>
                                                                                </table>
                                                                            </td>
                                                                        </tr>
                                                                    </table>
                                                                    <p> Please provide payment confirmation or contact us at your earliest convenience by replying to this email or at <a
                                                                            href=""mailto:oscar.alfaro@oceanscode.com"">oscar.alfaro@oceanscode.com</a>.
                                                                        To prevent service disruption.</p>
                                                                    <table cellspacing=""0"" cellpadding=""0"" width=""100%"">
                                                                        <tr>
                                                                            <td style=""text-align: left;"">
                                                                                <p style=""margin-top: 8px; margin-bottom: 5px;"">
                                                                                    Thank you,
                                                                                </p>
                                                                                <p style=""margin-top: 2px;""> Oscar Alfaro,
                                                                                    Finance Manager.
                                                                                </p>
                                                                            </td>
                                                                        </tr>
                                                                    </table>
                                                                    <table cellspacing=""0"" cellpadding=""0"" width=""100%"">
                                                                        <tr>
                                                                            <td style=""text-align: left;"">
                                                                                <img src=""https://oceansapp.azurewebsites.net/img/accounting-firm.png""
                                                                                    width=""100%"" alt=""Accounting"" />
                                                                            </td>
                                                                        </tr>
                                                                    </table>
                                                                        <table cellspacing=""0"" cellpadding=""0"" width=""100%"">
                                                                        <tr>
                                                                            <td style=""text-align: left;"">
                                                                                <p style=""margin-top: 8px; margin-bottom: 5px;"">
                                                                                   <strong>This is an automated email, ignore if you are up to date with your payments.</strong>
                                                                                </p>
                                                                            </td>
                                                                        </tr>
                                                                    </table>
                                                                </td>
                                                            </tr>
                                                        </table>
                                                    </div>
                                                </div>
                                            </td>
                                        </tr>
                                    </table>
                                </td>
                            </tr>
                        </table>
                    </td>
                </tr>
            </table>
            </td>
        </tr>
    </table>
</body>
</html>";
            return body;
        }

        private string emailBodyRed30Days(string clientName, decimal totalAmountDue, decimal documentAmount,
            string month,
            int year,
            string documentNumber,
            DateTime documentDate,
            DateTime docExpirationDate, int numDaysExpired, decimal latePaymentFee)
        {
            var note = "";
            if (latePaymentFee > 0)
            {
                note = @"<p><strong>Note that a late payment fee of " + (latePaymentFee * 100).ToString("#,##0") + @"% of the amount due was included because of the late payment</strong>
                        </p>";
            }
            var lateFeeApplied = "";
            if (latePaymentFee > 0)
            {
                lateFeeApplied = @"<tr>
                                   <td
                                       style=""border-right: 2px solid #9ba8b8; padding-right: 10px;"">
                                       <strong> LATE FEE AMOUNT:</strong>
                                   </td>
                                   <td
                                       style =""border-left: 2px solid #9ba8b8; padding-left: 10px;"">
                                       $" + (totalAmountDue * latePaymentFee).ToString("#,##0.00") + @"
                                   </td>
                               </tr> ";
            }
            var body = @"
<!DOCTYPE html>
<html>

<head>
</head>

<body>
    <table cellspacing=""0"" cellpadding=""0"" width=""100%"">
        <tr>
            <td>
                <table cellspacing=""0"" cellpadding=""0"" width=""500""
                style=""background-color: #fff; border: #000 solid 2px; padding: 10px;"">
                <tr>
                    <td>
                        <table cellspacing=""0"" cellpadding=""0"" width=""500"">
                            <tr>
                                <td align=""left"">
                                    <table cellspacing=""0"" cellpadding=""0"" width=""500""
                                        style=""background-color: #f5f6f7; margin: 0 auto; position: relative;z-index: 1; border-top-left-radius: 10px; border-top-right-radius: 10px;"">
                                        <tr>
                                            <td>
                                                <img src=""	https://oceansapp.azurewebsites.net/img/email/email-header-red.png""
                                                    alt=""Oceans Code Experts"" width=""100%"" />
                                            </td>
                                        </tr>
                                    </table>
                                    <table cellspacing=""0"" cellpadding=""0"" width=""500""
                                        style=""background-color: #f5f6f7; margin: 0 auto; border-bottom-left-radius: 10px; border-bottom-right-radius: 10px; position: relative; padding: 0 30px 30px 30px"">
                                        <tr>
                                            <td>
                                                <div>
                                                    <div>
                                                        <table cellspacing=""0"" cellpadding=""0"" width=""100%""
                                                            style=""font-family: 'nexa', Arial, sans-serif;"">
                                                            <tr>
                                                                <td>
                                                                    <table cellspacing=""0"" cellpadding=""0"" width=""100%"">
                                                                        <tr>
                                                                            <td>
                                                                                <hr style=""margin:0"" />
                                                                                <h2 style=""text-align: left; margin-bottom: 0;"">OVERDUE PAYMENT REQUIRED</h2>
                                                                            </td>
                                                                        </tr>
                                                                    </table>
                                                                    <p>Dear " + clientName + @",</p>
                                                                    <p>Your immediate attention is needed regarding an overdue balance of 
                                                                        <strong>$" + (totalAmountDue + (totalAmountDue * latePaymentFee)).ToString("#,##0.00") + @"</strong> for services
                                                                        delivered in the
                                                                        period of <strong>" + month + @" " + year + @"</strong>.
                                                                    </p>
                                                                    " + note + @"
                                                                    <table cellspacing=""0"" cellpadding=""0"" width=""100%"">
                                                                        <tr>
                                                                            <td
                                                                                style=""border-left: 8px solid red; padding-left: 10px; margin-top: 20px;"">
                                                                                <p
                                                                                    style=""color: red; margin: 0 0 10px 0; font-size: 18px;"">
                                                                                    <strong> INVOICE
                                                                                        NUMBER:</strong> " + documentNumber + @"
                                                                                </p>
                                                                                <table cellspacing=""0"" cellpadding=""0""
                                                                                    style=""font-size: 14px;"">
                                                                                    <tr>
                                                                                        <td
                                                                                            style=""border-right: 2px solid #9ba8b8; padding-right: 10px;"">
                                                                                            <strong> TOTAL AMOUNT</strong>
                                                                                        </td>
                                                                                        <td
                                                                                            style=""border-left: 2px solid #9ba8b8; padding-left: 10px;"">
                                                                                            $" + documentAmount.ToString("#,##0.00") + @"
                                                                                        </td>
                                                                                    </tr>
                                                                                  " + lateFeeApplied + @"
                                                                                    <tr>
                                                                                        <td
                                                                                            style=""border-right: 2px solid #9ba8b8; padding-right: 10px;"">
                                                                                            <strong> TOTAL AMOUNT DUE</strong>
                                                                                        </td>
                                                                                        <td
                                                                                            style=""border-left: 2px solid #9ba8b8; padding-left: 10px;"">
                                                                                            $" + (totalAmountDue + (totalAmountDue * latePaymentFee)).ToString("#,##0.00") + @"
                                                                                        </td>
                                                                                    </tr>
                                                                                    <tr>
                                                                                        <td
                                                                                            style=""border-right: 2px solid #9ba8b8; padding-right: 10px;"">
                                                                                            <strong>DATE</strong>
                                                                                        </td>
                                                                                        <td
                                                                                            style=""border-left: 2px solid #9ba8b8; padding-left: 10px;"">
                                                                                            " + documentDate.ToString("MM/dd/yyyy") + @"
                                                                                        </td>
                                                                                    </tr>
                                                                                    <tr>
                                                                                        <td
                                                                                            style=""border-right: 2px solid #9ba8b8; padding-right: 10px;"">
                                                                                            <strong> EXPIRATION DATE</strong>
                                                                                        </td>
                                                                                        <td
                                                                                            style=""border-left: 2px solid #9ba8b8; padding-left: 10px;"">
                                                                                            <span
                                                                                                style=""color: red;"">" + docExpirationDate.ToString("MM/dd/yyyy") + @"</span>
                                                                                        </td>
                                                                                    </tr>
                                                                                    <tr>
                                                                                        <td
                                                                                            style=""border-right: 2px solid #9ba8b8; padding-right: 10px;"">
                                                                                            <strong> DAYS EXPIRED</strong>
                                                                                        </td>
                                                                                        <td
                                                                                            style=""border-left: 2px solid #9ba8b8; padding-left: 10px;"">
                                                                                            <span
                                                                                                style=""color: red;"">" + numDaysExpired + @"</span>
                                                                                        </td>
                                                                                    </tr>
                                                                                </table>
                                                                            </td>
                                                                        </tr>
                                                                    </table>
                                                                    <p> Please provide payment confirmation or contact us at your earliest convenience by replying to this email or at <a
                                                                            href=""mailto:oscar.alfaro@oceanscode.com"">oscar.alfaro@oceanscode.com</a>.
                                                                        To prevent service disruption.</p>
                                                                    <table cellspacing=""0"" cellpadding=""0"" width=""100%"">
                                                                        <tr>
                                                                            <td style=""text-align: left;"">
                                                                                <p style=""margin-top: 8px; margin-bottom: 5px;"">
                                                                                    Thank you,
                                                                                </p>
                                                                                <p style=""margin-top: 2px;""> Oscar Alfaro,
                                                                                    Finance Manager.
                                                                                </p>
                                                                            </td>
                                                                        </tr>
                                                                    </table>
                                                                    <table cellspacing=""0"" cellpadding=""0"" width=""100%"">
                                                                        <tr>
                                                                            <td style=""text-align: left;"">
                                                                                <img src=""https://oceansapp.azurewebsites.net/img/accounting-firm.png""
                                                                                    width=""100%"" alt=""Accounting"" />
                                                                            </td>
                                                                        </tr>
                                                                    </table>
                                                                        <table cellspacing=""0"" cellpadding=""0"" width=""100%"">
                                                                        <tr>
                                                                            <td style=""text-align: left;"">
                                                                                <p style=""margin-top: 8px; margin-bottom: 5px;"">
                                                                                   <strong>This is an automated email, ignore if you are up to date with your payments.</strong>
                                                                                </p>
                                                                            </td>
                                                                        </tr>
                                                                    </table>
                                                                </td>
                                                            </tr>
                                                        </table>
                                                    </div>
                                                </div>
                                            </td>
                                        </tr>
                                    </table>
                                </td>
                            </tr>
                        </table>
                    </td>
                </tr>
            </table>
            </td>
        </tr>
    </table>
</body>
</html>";
            return body;
        }

    }
}
