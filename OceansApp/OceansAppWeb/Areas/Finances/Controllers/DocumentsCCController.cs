using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.Models;
using OceansApp.Models.ViewModels.Components;
using OceansApp.Models.ViewModels.DocumentsCC;
using OceansApp.Utility;
using OceansApp.Utility.Email;
using System.Security.Claims;

namespace OceansAppWeb.Areas.Finances.Controllers
{
    [Area("Finances")]
    [RequireTwoFactorEnabled]
    [Authorize(Roles = SD.Role_User_Master)]
    public class DocumentsCCController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISendEmailRepository _sendEmail;
        private readonly IConfiguration _config;
        public DocumentsCCController(IUnitOfWork unitOrWork, ISendEmailRepository sendEmail, IConfiguration config)
        {
            _unitOfWork = unitOrWork;
            _sendEmail = sendEmail;
            _config = config;
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

                var documentTypes = (List<SelectVM>?)_unitOfWork.DocumentCC.GetDocumentsTypeWhereDocumentsExist();
                List<SelectVM> documentTypesList = new List<SelectVM>();
                if (documentTypes != null)
                {
                    foreach (var docType in documentTypes)
                    {
                        documentTypesList.Add(new SelectVM { Value = docType.Value, Name = docType.Name });
                    }
                }

                var clients = _unitOfWork.Client.GetAll(x => x.ClientCode != "OCELL_C0001"
                && x.ClientCode != "OCE_C0028" && x.ClientCode != "OCE_C0029" && x.ClientCode != "OCE_C0030").OrderBy(x => x.Name);
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

        [HttpPost]
        public IActionResult SendNotification(int documentId)
        {
            try
            {
                var documentCC = _unitOfWork.DocumentCC.GetFirstOrDefault(x => x.DocumentCCId == documentId);
                var client = documentCC != null ? _unitOfWork.Client.GetFirstOrDefault(x => x.ClientId == documentCC.ClientId) : null;
                var notificationType = _unitOfWork.NotificationType.GetFirstOrDefault(x => x.Name == "Cuentas por cobrar");
                var notificationMedia = _unitOfWork.NotificationMedia.GetFirstOrDefault(x => x.Name == "Email");

                if (documentCC == null || client == null || notificationType == null || notificationMedia == null)
                {
                    return Json(new { success = false, error = "Error en la obtención de datos." });
                }

                string documentMonth = documentCC.DocumentDate.ToString("MMMM");
                DateTime docExpirationDate = documentCC.DocumentDate.AddDays(double.Parse(client.PaymentCondition));
                var subject = $"Invoice from {documentMonth} is still pending payment.";

                var allEmails = client.Emails.Split(new[] { ';', ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                var emailTo = allEmails.FirstOrDefault();
                var emailsCC = allEmails.Skip(1).ToList();
                var costaRicaTime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, "Central America Standard Time");
                int numDaysExpired = (int)(costaRicaTime - documentCC.DocumentDate).TotalDays - int.Parse(client.PaymentCondition);
                emailsCC.Add("oscar.alfaro@oceanscode.com");
                var body = "";
                emailsCC.Add("eder.rodriguez@oceanscode.com");
                emailsCC.Add("priscila.zamora@oceanscode.com");

                if (string.IsNullOrEmpty(emailTo))
                {
                    return Json(new { success = false, error = "El cliente no tiene correos electrónicos." });
                }
                var alreadyNotificationSent = _unitOfWork.DocumentsCCNotification.GetAll(x=>x.DocumentCCId == documentId);
                if (alreadyNotificationSent.Count() > 0)
                {
                   body = emailBodyRed(
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
                else
                {
                   body = emailBodyYellow(
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
                  
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

                var notification = new Notification()
                {
                    NotificationTypeId = notificationType.NotificationTypeId,
                    Body = body,
                    Subject = subject,
                    Remitent = _config["internalEmail"],
                    SentDate = costaRicaTime,
                    SentByUser = claim.Value
                };
                _unitOfWork.Notification.Add(notification);
                _unitOfWork.Save();

                var emailToSend = new SendEmailVM()
                {
                    Subject = subject,
                    Body = body,
                    EmailTo = emailTo,
                    SharedEmailFrom = _config["internalEmail"],
                    EmailCcList = emailsCC
                };

                var notificationStatus = _unitOfWork.NotificationStatus.GetFirstOrDefault(x => x.Name == "Enviado");
                if (notificationStatus == null)
                {
                    return Json(new { success = false, error = "Error en la obtención de datos." });
                }

                try
                {
                    var emailSent = _sendEmail.SendEmail(emailToSend);
                    var documentNotification = new DocumentsCCNotification()
                    {
                        DocumentCCId = documentId,
                        NotificationId = notification.NotificationId
                    };
                    _unitOfWork.DocumentsCCNotification.Add(documentNotification);
                    _unitOfWork.Save();
                }
                catch (Exception ex)
                {
                    notificationStatus = _unitOfWork.NotificationStatus.GetFirstOrDefault(x => x.Name == "Envío fallido");
                }
                var recipientUserSendTo = _unitOfWork.ApplicationUser.GetFirstOrDefault(x => x.Email == emailTo);
                var recipientUserIdSendTo = recipientUserSendTo?.Id;
                var notificationRecipientSentTo = new NotificationRecipient()
                {
                    RecipientMediaInfo = emailTo,
                    NotificationId = notification.NotificationId,
                    NotificationMediaId = notificationMedia.NotificationMediaId,
                    NotificationStatusId = notificationStatus.NotificationStatusId,
                    RecipientUserId = recipientUserIdSendTo
                };
                _unitOfWork.NotificationRecipient.Add(notificationRecipientSentTo);
                foreach (var email in emailsCC)
                {
                    var recipientUserCC = _unitOfWork.ApplicationUser.GetFirstOrDefault(x => x.Email == email);
                    var recipientUserIdCC = recipientUserCC?.Id;
                    var notificationRecipientCC = new NotificationRecipient()
                    {
                        RecipientMediaInfo = email,
                        NotificationId = notification.NotificationId,
                        NotificationMediaId = notificationMedia.NotificationMediaId,
                        NotificationStatusId = notificationStatus.NotificationStatusId,
                        RecipientUserId = recipientUserIdCC
                    };
                    _unitOfWork.NotificationRecipient.Add(notificationRecipientCC);
                }
                _unitOfWork.Save();
                return Json(new { success = true, message = $"¡Bien, le acabas de enviar una notificación a {client.Name}." });
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
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

        private string emailBodyYellow(string clientName, decimal totalAmountDue, decimal documentAmount,
            string month,
            int year,
            string documentNumber,
            DateTime documentDate,
            DateTime docExpirationDate, int numDaysExpired)
        {
            var body = @"<!DOCTYPE html>
<html>
<head>
</head>
<body>
    <div style=""max-width: 600px;
        margin: 0 auto;
        background-color: #f5f6f7;
        border-radius: 10px;
        position: relative;
        box-shadow: 5px 5px 5px rgba(0, 0, 0, 0.5);"">
         <img src=""https://res.cloudinary.com/oceans-consulting-firm/image/upload/v1693322491/logos/email-header.png""
            alt=""Oceans Code Experts"" width=""100%""
            style=""position: absolute; top: 0; margin-left: -30px; border-top-left-radius: 10px; border-top-right-radius: 10px;"" />
       <div style=""padding: 30px;"">
            <table cellspacing=""0"" cellpadding=""0"" width=""100%"">
                 <tr>
                     <td>
                         <table cellspacing=""0"" cellpadding=""0"" width=""100%"">
                                    <tr>
                                        <td>
                                            <hr />
                                            <h2 style=""text-align: left;""> OVERDUE INVOICE PAYMENT</h2>
                                            </td>
                                        </tr>
                                    </table>
                                    <p>Dear " + clientName + @",</p>
                                    <p>Your attention is needed regarding an overdue balance of <strong>$" + totalAmountDue.ToString("#,##0.00")
                        + @"</strong> for services delivered in the period of <strong>" + month + @" " + year + @".</strong></p>
                    <table cellspacing=""0"" cellpadding=""0"" width=""100%"">
                        <tr>
                            <td style=""border-left: 8px solid #eeb30f; padding-left: 10px; margin-top: 20px;"">
                                 <p style=""color: #eeb30f; margin: 0 0 10px 0; font-size: 18px;""><strong> INVOICE
                                        NUMBER:</strong> " + documentNumber + @" </p>
                                <table cellspacing=""0"" cellpadding=""0"" style=""font-size: 14px;"">
                                         <tr>
                                             <td style=""border-right: 2px solid #9ba8b8; padding-right: 10px;"">
                                                  <strong> TOTAL AMOUNT</strong>
                                                </td>
                                                <td style=""border-left: 2px solid #9ba8b8; padding-left: 10px;"">
                                                     $" + documentAmount.ToString("#,##0.00") + @"
                                        </td>
                                             </tr>
                                             <tr>
                                                 <td style=""border-right: 2px solid #9ba8b8; padding-right: 10px;"">
                                                      <strong> TOTAL AMOUNT DUE</strong>
                                                    </td>
                                                    <td style=""border-left: 2px solid #9ba8b8; padding-left: 10px;"">
                                                         $" + totalAmountDue.ToString("#,##0.00") + @"
                                        </td>
                                                 </tr>
                                                 <tr>
                                                     <td style=""border-right: 2px solid #9ba8b8; padding-right: 10px;"">
                                                          <strong>DATE</strong>
                                                        </td>
                                                        <td style=""border-left: 2px solid #9ba8b8; padding-left: 10px;"">
                                                             " + documentDate.ToString("MM/dd/yyyy") + @"
                                                             </td>
                                                         </tr>
                                                         <tr>
                                                             <td style=""border-right: 2px solid #9ba8b8; padding-right: 10px;"">
                                                                  <strong> EXPIRATION DATE</strong>
                                                                  </td>
                                                                  <td style=""border-left: 2px solid #9ba8b8; padding-left: 10px;"">
                                                                       <span style=""color: red;""> " + docExpirationDate.ToString("MM/dd/yyyy") +
                                                @"</span>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td style=""border-right: 2px solid #9ba8b8; padding-right: 10px;"">
                                            <strong> DAYS EXPIRED</strong>
                                        </td>
                                        <td style=""border-left: 2px solid #9ba8b8; padding-left: 10px;"">
                                             <span style=""color: red;""> " + numDaysExpired + @" </span>
                                          </td>
                                      </tr>
                                  </table>
                              </td>
                          </tr>
                      </table>
                      <p> Please return confirmation upon receipt of this message and let us know if you need further
                        clarification or assistance by replying to this email or at <a href=""mailto:oscar.alfaro@oceanscode.com"">oscar.alfaro@oceanscode.com</a>. Your
                         prompt response is appreciated.</p>
                     <table cellspacing=""0"" cellpadding=""0"" width=""100%"">
                              <tr>
                                  <td style=""text-align: left;"">
                                       <p style=""margin-top: 8px; margin-bottom: 5px;""> Thank you,</p>
                                            <p style=""margin-top: 2px;""> Oscar Alfaro, Finance Manager.</p>
                                             </td>
                                         </tr>
                                     </table>
                                     <table cellspacing=""0"" cellpadding=""0"" width=""100%"">
                                              <tr>
                                                  <td style=""text-align: left;"">
                                                       <img width=""100%""
                                    src=""https://res.cloudinary.com/oceans-consulting-firm/image/upload/v1677609596/accounting-system/Firma_Accounting.png""
                                    alt=""Accounting"" />
                            </td>
                        </tr>
                    </table>
                </td>
            </tr>
        </table>
       </div>
    </div>
</body>
</html>";
            return body;
        }

        private string emailBodyRed(string clientName, decimal totalAmountDue, decimal documentAmount,
            string month,
            int year,
            string documentNumber,
            DateTime documentDate,
            DateTime docExpirationDate, int numDaysExpired)
        {
            var body = @"<!DOCTYPE html>
<html>
<head>
</head>
<body>
    <div style=""max-width: 600px;
        margin: 0 auto;
        background-color: #f5f6f7;
        border-radius: 10px;
        position: relative;
        box-shadow: 5px 5px 5px rgba(0, 0, 0, 0.5);"">
         <img src=""https://res.cloudinary.com/oceans-consulting-firm/image/upload/v1693329272/logos/email-header-red.png""
            alt=""Oceans Code Experts"" width=""100%""
            style=""position: absolute; top: 0; margin-left: -30px; border-top-left-radius: 10px; border-top-right-radius: 10px;"" />
       <div style=""padding: 30px;"">
            <table cellspacing=""0"" cellpadding=""0"" width=""100%"">
                 <tr>
                     <td>
                         <table cellspacing=""0"" cellpadding=""0"" width=""100%"">
                                    <tr>
                                        <td>
                                            <hr />
                                            <h2 style=""text-align: left;""> OVERDUE PAYMENT REQUIRED</h2>
                                            </td>
                                        </tr>
                                    </table>
                                    <p>Dear " + clientName + @",</p>
                                    <p>Your immediate attention is needed regarding an overdue balance of <strong>$" + totalAmountDue.ToString("#,##0.00")
                        + @"</strong> for services delivered in the period of <strong>" + month + @" " + year + @".</strong></p>
                    <table cellspacing=""0"" cellpadding=""0"" width=""100%"">
                        <tr>
                            <td style=""border-left: 8px solid red; padding-left: 10px; margin-top: 20px;"">
                                 <p style=""color: red; margin: 0 0 10px 0; font-size: 18px;""><strong> INVOICE
                                        NUMBER:</strong> " + documentNumber + @" </p>
                                <table cellspacing=""0"" cellpadding=""0"" style=""font-size: 14px;"">
                                         <tr>
                                             <td style=""border-right: 2px solid #9ba8b8; padding-right: 10px;"">
                                                  <strong> TOTAL AMOUNT</strong>
                                                </td>
                                                <td style=""border-left: 2px solid #9ba8b8; padding-left: 10px;"">
                                                     $" + documentAmount.ToString("#,##0.00") + @"
                                        </td>
                                             </tr>
                                             <tr>
                                                 <td style=""border-right: 2px solid #9ba8b8; padding-right: 10px;"">
                                                      <strong> TOTAL AMOUNT DUE</strong>
                                                    </td>
                                                    <td style=""border-left: 2px solid #9ba8b8; padding-left: 10px;"">
                                                         $" + totalAmountDue.ToString("#,##0.00") + @"
                                        </td>
                                                 </tr>
                                                 <tr>
                                                     <td style=""border-right: 2px solid #9ba8b8; padding-right: 10px;"">
                                                          <strong>DATE</strong>
                                                        </td>
                                                        <td style=""border-left: 2px solid #9ba8b8; padding-left: 10px;"">
                                                             " + documentDate.ToString("MM/dd/yyyy") + @"
                                                             </td>
                                                         </tr>
                                                         <tr>
                                                             <td style=""border-right: 2px solid #9ba8b8; padding-right: 10px;"">
                                                                  <strong> EXPIRATION DATE</strong>
                                                                  </td>
                                                                  <td style=""border-left: 2px solid #9ba8b8; padding-left: 10px;"">
                                                                       <span style=""color: red;""> " + docExpirationDate.ToString("MM/dd/yyyy") +
                                                @"</span>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td style=""border-right: 2px solid #9ba8b8; padding-right: 10px;"">
                                            <strong> DAYS EXPIRED</strong>
                                        </td>
                                        <td style=""border-left: 2px solid #9ba8b8; padding-left: 10px;"">
                                             <span style=""color: red;""> " + numDaysExpired + @" </span>
                                          </td>
                                      </tr>
                                  </table>
                              </td>
                          </tr>
                      </table>
                      <p> Please provide payment confirmation or contact us at your earliest convenience by replying to this email or at <a href=""mailto:oscar.alfaro@oceanscode.com"">oscar.alfaro@oceanscode.com</a>. To
                         prevent service disruption.</p>
                     <table cellspacing=""0"" cellpadding=""0"" width=""100%"">
                              <tr>
                                  <td style=""text-align: left;"">
                                       <p style=""margin-top: 8px; margin-bottom: 5px;""> Thank you,</p>
                                            <p style=""margin-top: 2px;""> Oscar Alfaro, Finance Manager.</p>
                                             </td>
                                         </tr>
                                     </table>
                                     <table cellspacing=""0"" cellpadding=""0"" width=""100%"">
                                              <tr>
                                                  <td style=""text-align: left;"">
                                                       <img width=""100%""
                                    src=""https://res.cloudinary.com/oceans-consulting-firm/image/upload/v1677609596/accounting-system/Firma_Accounting.png""
                                    alt=""Accounting"" />
                            </td>
                        </tr>
                    </table>
                </td>
            </tr>
        </table>
       </div>
    </div>
</body>
</html>";
            return body;
        }

    }
}
