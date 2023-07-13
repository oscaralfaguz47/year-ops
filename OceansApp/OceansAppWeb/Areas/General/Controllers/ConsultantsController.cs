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
            ProviderFiltersGetAllVM filters = new ProviderFiltersGetAllVM
            {
                Pagination = new Pagination
                {
                    PageNumber = 1,
                    PageSize = 10
                },
                IsActive = "S"
            };

            var consultants = await _unitOfWork.Provider.GetAllProviderWithFiltersAsync(filters);

            ProviderFiltersGetAllVM viewModel = new ProviderFiltersGetAllVM
            {
                IsActive = filters.IsActive,
                NameOrAlias = filters.NameOrAlias,
                CountryId = filters.CountryId,
                ClientId = filters.ClientId,
                CompanyId = filters.CompanyId,
                Pagination = filters.Pagination,
                ConsultantList = consultants
            };

            return View(viewModel);
        }
    }
}
