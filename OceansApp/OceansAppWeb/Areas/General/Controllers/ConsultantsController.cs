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
        public async Task<IActionResult> Index()
        {
            ProviderGetAllForListVM model = new ProviderGetAllForListVM
            {
                Pagination = new Pagination
                {
                    PageNumber = 1,
                    PageSize = 10
                },
                Filters = new ProviderFiltersGetAllVM
                {
                    IsActive = "S"
                }
            };
            var result = await _unitOfWork.Provider.GetAllProviderWithFiltersAsync(model);

            model.Pagination.TotalResults = result.totalCount;

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
