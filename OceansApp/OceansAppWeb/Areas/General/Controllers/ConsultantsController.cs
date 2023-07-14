using Microsoft.AspNetCore.Mvc;
using OceansApp.DataAccess.Repository.IRepository;
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
        public async Task<IActionResult> Index(int page = 1, int pageSize = 10)
        {
            ProviderGetAllForListVM model = new ProviderGetAllForListVM
            {
                Pagination = new Pagination
                {
                    PageNumber = page,
                    PageSize = pageSize,
                    PageSizeOptions = new List<int> { 10, 30, 50, 100 },
                    SelectedPageSize = pageSize
                },
                Filters = new ProviderFiltersGetAllVM
                {
                }
            };
            var result = await _unitOfWork.Provider.GetAllProviderWithFiltersAsync(model);

            model.Pagination.TotalResults = result.totalCount;

            var totalPages = (int)Math.Ceiling((double)model.Pagination.TotalResults / model.Pagination.PageSize);
            model.Pagination.PageNumber = Math.Max(1, Math.Min(model.Pagination.PageNumber, totalPages));

            ProviderGetAllForListVM viewModel = new ProviderGetAllForListVM
            {
                ConsultantList = result.providers,
                Pagination = model.Pagination,
                Filters = model.Filters
            };

            return View(viewModel);
        }


    }
}
