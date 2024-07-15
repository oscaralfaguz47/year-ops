using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.ViewModels.Components;

namespace OceansAppWeb.Areas.AdminCenter.Controllers
{
    [ApiController]
    [Route("AdminCenter/[controller]")]
    [Area("AdminCenter")]
    [EnableCors("AllowSpecificOrigin")]
    [ServiceFilter(typeof(RequireTwoFactorEnabledAttribute))]
    [Authorize]
    public class CountriesController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public CountriesController(IUnitOfWork unitOrWork)
        {
            _unitOfWork = unitOrWork;
        }

        [Authorize(Policy = "AccessToAllCountriesList")]
        [HttpGet("GetAllCountriesListForSelect")]
        public async Task<IActionResult> GetAllCountriesListForSelect()
        {
            try
            {
                List<SelectVM> countriesList = new();
                var countries = await _unitOfWork.Country.GetAllAsync();
                foreach (var country in countries)
                {
                    countriesList.Add(new SelectVM { Value = country.IdCountry, Text = country.Name });
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
