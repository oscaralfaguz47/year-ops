using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace OceansAppWeb.Controllers
{
    [Authorize]
    [ServiceFilter(typeof(RequireTwoFactorEnabledAttribute))]
    public class PaginationController : Controller
    {
        public IActionResult GetPagination()
        {
            return PartialView("_Pagination");
        }
    }
}
