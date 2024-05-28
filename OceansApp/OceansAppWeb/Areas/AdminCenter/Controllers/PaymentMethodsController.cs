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
    [Authorize]
    public class PaymentMethodsController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public PaymentMethodsController(IUnitOfWork unitOrWork)
        {
            _unitOfWork = unitOrWork;
        }

        [Authorize(Policy = "AccessToPaymentMethodsList")]
        [HttpGet]
        public async Task<IActionResult> GetPaymentMethodsListWhereCompany(string companyId)
        {
            try
            {
                List<GetDataForSelectVM> paymentMethodsList = new();
                var paymentMethods = _unitOfWork.PaymentMethod.GetAll().Where(x => x.CompanyId == companyId);
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
    }
}
