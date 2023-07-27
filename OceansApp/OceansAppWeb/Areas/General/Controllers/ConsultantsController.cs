using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.ViewModels.Components;
using OceansApp.Models.ViewModels.Providers;
using OceansApp.Utility;

namespace OceansAppWeb.Areas.General.Controllers
{
    [Area("General")]
    [RequireTwoFactorEnabled]
    [Authorize(Roles = SD.Role_User_Master)]
    public class ConsultantsController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public ConsultantsController(IUnitOfWork unitOrWork)
        {
            _unitOfWork = unitOrWork;
        }

        public async Task<IActionResult> Index(ProviderGetAllForListVM model)
        {
            try
            {
                ProviderFiltersGetAllVM filtersToSend = new ProviderFiltersGetAllVM();
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
                    filtersToSend.IsActive = null;
                    filtersToSend.CompanyId = null;
                }
                else
                {
                    filtersToSend = model.Filters;
                }


                ProviderGetAllForListVM modelToSend = new ProviderGetAllForListVM
                {
                    Pagination = paginationToSend,
                    Filters = filtersToSend

                };
                var countries = _unitOfWork.Country.GetCountriesWhereConsultantsAre();
                List<SelectVM> countriesList = new List<SelectVM>();
                if (countries != null)
                {
                    foreach (var country in countries)
                    {
                        countriesList.Add(new SelectVM { Value = country.IdCountry, Name = country.Name });
                    }
                }
                var clients = _unitOfWork.Client.GetAll(x => x.ClientCategory == "EXT" && x.ClientCode != "OCELL_C0001"
                && x.ClientCode != "OCE_C0028" && x.ClientCode != "OCE_C0029" && x.ClientCode != "OCE_C0030").OrderBy(x => x.Name);
                List<SelectVM> clientList = new List<SelectVM>();
                if (clients != null)
                {
                    foreach (var client in clients)
                    {
                        clientList.Add(new SelectVM { Value = client.ClientId.ToString(), Name = client.Name });
                    }
                }

                var totalResults = await _unitOfWork.Provider.GetAllProviderWithFiltersAsync(modelToSend);

                int totalNum = totalResults.totalCount;

                int totalPages = (int)Math.Ceiling(totalNum / (double)modelToSend.Pagination.PageSize);

                ViewData["TotalPages"] = totalPages;

                modelToSend.Pagination.PageIndex = Math.Max(1, Math.Min(modelToSend.Pagination.PageIndex, totalPages));

                modelToSend.Pagination.TotalResults = totalResults.totalCount;

                ProviderGetAllForListVM viewModel = new ProviderGetAllForListVM
                {
                    ConsultantList = totalResults.providers,
                    Pagination = modelToSend.Pagination,
                    Filters = modelToSend.Filters,
                    CountriesList = countriesList,
                    ClientList = clientList
                };
                return View(viewModel);
            }catch (Exception ex)
            {
                return View(ex);
                Console.WriteLine(ex.ToString());
            }
        }

        private bool WhereFiltersApplied(ProviderFiltersGetAllVM model1, ProviderFiltersGetAllVM model2)
        {
            return !(model1.IsActive == model2.IsActive && model1.CountryId == model2.CountryId
                && model1.ClientId == model2.ClientId && model1.CompanyId == model2.CompanyId);
        }


    }
}
