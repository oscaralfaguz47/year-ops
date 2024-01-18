using Microsoft.AspNetCore.Mvc;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.ViewModels.Clients;
using OceansApp.Models.ViewModels.Components;

namespace OceansAppWeb.Areas.ProjectManagement.Controllers
{
    [Area("ProjectManagement")]
    [RequireTwoFactorEnabled]
    public class ClientsController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public ClientsController(IUnitOfWork unitOrWork)
        {
            _unitOfWork = unitOrWork;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetClientsList(string model)
        {
            try
            {
                ClientsPaginationFiltersVM clientsPaginationFilters = System.Text.Json.JsonSerializer.Deserialize<ClientsPaginationFiltersVM>(model);

                ClientsPaginationFiltersVM paginationFilters = new ClientsPaginationFiltersVM();
                paginationFilters.Filters = new ClientsFiltersGetAllVM();

                int numAppliedFilters = 0;
                if (clientsPaginationFilters.Filters != null)
                {
                    foreach (var prop in clientsPaginationFilters.Filters.GetType().GetProperties())
                    {
                        var value = prop.GetValue(clientsPaginationFilters.Filters, null);
                        if (value is not null and not "")
                        {
                            numAppliedFilters++;
                        }
                    }
                }
                var setPagination = new PaginationFiltersBehavior();
                paginationFilters.PaginationWithoutFilters = setPagination.SetPagination(clientsPaginationFilters.PaginationWithoutFilters, numAppliedFilters);

                if (numAppliedFilters > 0)
                {
                    paginationFilters.Filters = clientsPaginationFilters.Filters;
                }

                var totalResults = await _unitOfWork.Client.GetAllClientsWithFiltersAsync(paginationFilters);
                paginationFilters.PaginationWithoutFilters.Pagination.TotalResults = totalResults.totalCount;
                ClientsGetAllForListVM viewModel = new ClientsGetAllForListVM
                {
                    ClientsList = totalResults.clients,
                    PaginationFilters = paginationFilters
                };
                string jsonResult = System.Text.Json.JsonSerializer.Serialize(viewModel);
                return Content(jsonResult, "application/json");
            }
            catch (Exception ex)
            {
                return BadRequest(new { errors = new[] { $"There was an error fetching the list of clients." }, result = "errorGet", detail = ex.Message });
            }
        }
    }
}
