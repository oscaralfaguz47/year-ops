using Microsoft.AspNetCore.Authorization;
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
        public DocumentsCCController(IUnitOfWork unitOrWork)
        {
            _unitOfWork = unitOrWork;
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
            }catch (Exception ex)
            {
                TempData["error"] = "¡Algo salió mal!";
                return RedirectToAction("Index");
            }
        }

        private bool WhereFiltersApplied(DocumentCCFiltersGetAllVM model1, DocumentCCFiltersGetAllVM model2)
        {
            return !(model1.DocumentType == model2.DocumentType && model1.StartDate == model2.StartDate
                && model1.EndDate == model2.EndDate && model1.ClientId == model2.ClientId
                && model1.CompanyId == model2.CompanyId);
        }

    }
}
