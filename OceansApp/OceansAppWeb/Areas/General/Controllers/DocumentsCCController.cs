using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.ViewModels.Components;
using OceansApp.Models.ViewModels.DocumentsCC;
using OceansApp.Utility;

namespace OceansAppWeb.Areas.General.Controllers
{
    [Area("General")]
    [RequireTwoFactorEnabled]
    [Authorize(Roles = SD.Role_User_Master)]
    public class DocumentsCCController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailSender _emailSender;
        public DocumentsCCController(IUnitOfWork unitOrWork, IEmailSender emailSender)
        {
            _unitOfWork = unitOrWork;
            _emailSender = emailSender;
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
                        if (WhereFiltersApplied(model.Filters, filtersToSend2))
                        {
                            ViewData["AppliedFilters"] = "filters where applied";
                        }
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

                if (model.Filters != null)
                {
                    if (WhereFiltersApplied(model.Filters, filtersToSend))
                    {
                        ViewData["AppliedFilters"] = "filters where applied";
                    }
                }
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

        //POST
        [HttpPost]
        public IActionResult SendNotification(int documentId)
        {
            try
            {
                var documentCC = _unitOfWork.DocumentCC.GetFirstOrDefault(x => x.DocumentCCId == documentId);
                if (documentCC == null)
                {
                    return BadRequest("El documento no fue encontrado.");
                }

                var client = _unitOfWork.Client.GetFirstOrDefault(x => x.ClientId == documentCC.ClientId);
                if (client == null)
                {
                    return BadRequest("El cliente no fue encontrado.");
                }
                var subject = "Invoice from June is still pending payment";
                string nombreMes = documentCC.DocumentDate.ToString("MMM");
                var body = emailBody();

                var emailSent = _emailSender.SendEmailAsync("oscar.alfaro@oceanscode.com", subject, body);

                return Json(new { success = true, message = "¡Bien, le acabas de enviar una notificación al cliente!" });
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        private string emailBody()
        {
            var body = @"<!DOCTYPE html>
                        <html>
                        <head>
                        <style>
                          body {
                            font-family: Arial, sans-serif;
                          }
                          .container {
                            max-width: 600px;
                            margin: 0 auto;
                            padding: 20px;
                            border: 1px solid #ccc;
                          }
                          .header {
                            text-align: center;
                            margin-bottom: 20px;
                          }
                          .invoice-details {
                            border: 1px solid #ccc;
                            padding: 10px;
                            margin-top: 20px;
                          }
                          .signature {
                            text-align: left;
                            margin-top: 20px;
                          }
                          .red-text{
                            color: red;
                          }
                        </style>
                        </head>
                        <body>
                        
                        <div class=""container"">
                          <div class=""header"">
                           <div style=""display:inline-block; background-color:#fff;"">
                             <img src=""https://res.cloudinary.com/oceans-consulting-firm/image/upload/v1612882702/logos/logo-color_xdip1b.png"" alt=""Oceans Code Experts"" width=""200"" style=""display:block; margin:0 auto;"">
                             </div>
                                   <h2>INVOICE PENDING [June 2023]</h2>
                          </div>
                          <p>Dear Emilio,</p>
                          <p>We just want to remind you that there is currently an unpaid balance invoice for $1,000.00 corresponding to the month of June 2023.</p>
                          <p>Please see the information below:</p>
                          <div class=""invoice-details"">
                            <p><strong>Invoice Number:</strong> 225</p>
                            <p><strong>Amount:</strong> $1,000.00</p>
                            <p><strong>Date:</strong> 06/30/2023</p>
                            <p class=""red-text""><strong>Expiration Date:</strong> 07/07/2023</p>
                            <p><strong>Details:</strong> Professional Services</p>
                          </div>
                          <p>You can reply to this email or contact directly with the Finance Manager Oscar Alfaro at oscar.alfaro@oceanscode.com</p>
                          <p>Thanks!</p>
                          
                          <div class=""signature"">
                            <img src=""https://res.cloudinary.com/oceans-consulting-firm/image/upload/v1677609596/accounting-system/Firma_Accounting.png"" alt=""Accounting"">
                          </div>
                        </div>
                        
                        </body>
                        </html>";
            return body;
        }


        private bool WhereFiltersApplied(DocumentCCFiltersGetAllVM model1, DocumentCCFiltersGetAllVM model2)
        {
            return !(model1.DocumentType == model2.DocumentType && model1.StartDate == model2.StartDate
                && model1.EndDate == model2.EndDate && model1.ClientId == model2.ClientId
                && model1.CompanyId == model2.CompanyId);
        }

    }
}
