using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.ViewModels.Components;

namespace OceansAppWeb.Areas.AdminCenter.Controllers
{
    [Area("AdminCenter")]
    [EnableCors("AllowSpecificOrigin")]
    [RequireTwoFactorEnabled]
    public class CountriesController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public CountriesController(IUnitOfWork unitOrWork)
        {
            _unitOfWork = unitOrWork;
        }
        public IActionResult Index()
        {
            return View();
        }

        [Authorize(Policy = "AccessToAllCountriesList")]
        [HttpGet]
        public async Task<IActionResult> GetAllCountriesListForSelect()
        {
            try
            {
                List<SelectVM> countriesList = new();
                var countries = _unitOfWork.Country.GetAll();
                foreach (var country in countries)
                {
                    countriesList.Add(new SelectVM { Value = country.IdCountry, Name = country.Name });
                }
                return Ok(new
                {
                    Countries = countriesList
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = "Error retrieving data. Please report this issue.", detail = ex.Message });
            }
        }
    }
}
