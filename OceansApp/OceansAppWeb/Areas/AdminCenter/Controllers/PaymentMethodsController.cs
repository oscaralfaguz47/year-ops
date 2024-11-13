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
    public class PaymentMethodsController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public PaymentMethodsController(IUnitOfWork unitOrWork)
        {
            _unitOfWork = unitOrWork;
        }

        [Authorize(Policy = "AccessToPaymentMethodsList")]
        [HttpGet("GetPaymentMethodsListWhereCompany")]
        public async Task<IActionResult> GetPaymentMethodsListWhereCompany(string companyId)
        {
            try
            {
                List<GetDataForSelectVM> paymentMethodsList = new();
                var paymentMethods = (await _unitOfWork.PaymentMethod.GetAllAsync()).Where(x => x.CompanyId == companyId);
                foreach (var paymentMethod in paymentMethods)
                {
                    paymentMethodsList.Add(new GetDataForSelectVM { Value = paymentMethod.PaymentMethodId, Text = paymentMethod.Name });
                }
                return Ok(new
                {
                    PaymentMethods = paymentMethodsList
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = "Error retrieving data. Please report this issue.", detail = ex.Message });
            }
        }
        [Authorize(Policy = "AccessToPaymentMethodsList")]
        [HttpGet("GetAllPaymentMethodsList")]
        public async Task<IActionResult> GetAllPaymentMethodsList()
        {
            try
            {
                List<GetDataForSelectVM> paymentMethodsList = new();
                var paymentMethods = (await _unitOfWork.PaymentMethod.GetAllAsync());
                foreach (var paymentMethod in paymentMethods)
                {
                    paymentMethodsList.Add(new GetDataForSelectVM { Value = paymentMethod.PaymentMethodId, Text = paymentMethod.Name });
                }
                return Ok(new
                {
                    PaymentMethods = paymentMethodsList
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = "Error retrieving data. Please report this issue.", detail = ex.Message });
            }
        }

        [Authorize(Policy = "AccessToPaymentMethodsList")]
        [HttpGet("GetCompanyByPaymentMethod")]
        public async Task<IActionResult> GetCompanyByPaymentMethod(int paymentMethodId)
        {
            try
            {
                var paymentMethod = await _unitOfWork.PaymentMethod.GetFirstOrDefaultAsync(x => x.PaymentMethodId == paymentMethodId);
                if (paymentMethod == null)
                {
                    return NotFound(new { error = "The payment method was not found." });
                }
               
                return Ok(new
                {
                    CompanyId = paymentMethod.CompanyId
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = "Error retrieving data. Please report this issue.", detail = ex.Message });
            }
        }
    }
}
