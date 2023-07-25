using Microsoft.AspNetCore.Mvc;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.ViewModels.Components;
using OceansApp.Models.ViewModels.Providers;

namespace OceansAppWeb.Areas.General.Controllers
{
    [Area("General")]
    [RequireTwoFactorEnabled]
    public class ConsultantsController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public ConsultantsController(IUnitOfWork unitOrWork)
        {
            _unitOfWork = unitOrWork;
        }

        public async Task<IActionResult> Index(ProviderGetAllForListVM model)
        {
            ProviderFiltersGetAllVM filtersToSend = new ProviderFiltersGetAllVM();
            Pagination paginationToSend = new Pagination();

            if (model.Pagination == null)
            {
                paginationToSend = new Pagination();
            }
            else
            {
                paginationToSend = model.Pagination;

                if (model.Filters.NameOrAlias != filtersToSend.NameOrAlias)
                {
                    paginationToSend.PageIndex = 1;
                }
            }

            if (model.Filters == null)
            {
                filtersToSend.IsActive = null;
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
            var countries = _unitOfWork.Country.GetAll();
            List<SelectVM> countriesList = new List<SelectVM>();
            if (countries != null)
            {
                foreach (var country in countries)
                {
                    countriesList.Add(new SelectVM { Value = country.IdCountry, Name = country.Name });
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
                CountriesList = countriesList
            };
            return View(viewModel);
        }

       
    }
}
