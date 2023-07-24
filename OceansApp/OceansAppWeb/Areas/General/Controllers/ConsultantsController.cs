using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.ViewModels.Components;
using OceansApp.Models.ViewModels.Providers;
using OceansApp.Utility;
using System.Collections.ObjectModel;

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

        public async Task<IActionResult> Index(ProviderGetAllForListVM model, int page = 1, int pageSize = 30)
        {
            ProviderFiltersGetAllVM filtersToSend = new ProviderFiltersGetAllVM();
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
                Pagination = new Pagination
                {
                    PageNumber = page,
                    PageSize = pageSize,
                    PageSizeOptions = new List<int> { 30, 50, 100, 300 },
                    SelectedPageSize = pageSize
                },
                Filters = filtersToSend

            };
            var countries = _unitOfWork.Country.GetAll();
            List<SelectVM> countriesList = new List<SelectVM>();
            if (countries != null)
            {
                foreach (var country in countries)
                {
                    countriesList.Add(new SelectVM { Id = country.IdCountry, Name = country.Name });
                }
            }

            var result = await _unitOfWork.Provider.GetAllProviderWithFiltersAsync(modelToSend);

            modelToSend.Pagination.TotalResults = result.totalCount;

            var totalPages = (int)Math.Ceiling((double)modelToSend.Pagination.TotalResults / modelToSend.Pagination.PageSize);
            modelToSend.Pagination.PageNumber = Math.Max(1, Math.Min(modelToSend.Pagination.PageNumber, totalPages));

            ProviderGetAllForListVM viewModel = new ProviderGetAllForListVM
            {
                ConsultantList = result.providers,
                Pagination = modelToSend.Pagination,
                Filters = modelToSend.Filters,
                CountriesList = countriesList
            };
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult GetFilteredConsultants(ProviderGetAllForListVM model)
        {
            try
            {
                int pageNumber = 1;
                int pageSize = 30;
                List<int> pageSizeOptions = new List<int> { 30, 50, 100, 300 };
                ProviderGetAllForListVM modelToSend = new ProviderGetAllForListVM
                {
                    Pagination = new Pagination
                    {
                        PageNumber = pageNumber,
                        PageSize = pageSize,
                        PageSizeOptions = pageSizeOptions,
                        SelectedPageSize = pageSize
                    },
                    Filters = model.Filters
                };
                return View("Index", modelToSend);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}
