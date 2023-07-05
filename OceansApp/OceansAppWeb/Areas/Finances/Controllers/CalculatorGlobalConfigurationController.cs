using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.Models;
using OceansApp.Models.ViewModels;
using OceansApp.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.ObjectModel;
using System.Security.Claims;

namespace FinancialCalculatorWeb.Areas.Finances.Controllers
{
    [Area("Finances")]
    [Authorize(Roles = SD.Role_User_Master)]
    [RequireTwoFactorEnabled]
    public class CalculatorGlobalConfigurationController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public CalculatorGlobalConfigurationController(IUnitOfWork unitOrWork)
        {
            _unitOfWork = unitOrWork;
        }

        public IActionResult Index()
        {
            IEnumerable<CostCenter> costCenterList = (IEnumerable<CostCenter>)_unitOfWork.CenterOfCosts.GetCostCenterOfExpenses();
            CalculatorGlobalConfiguration currentConfig = _unitOfWork.CalculatorGlobalConfiguration.GetFirstOrDefault(x => x.Id == "Configuration1");
            if (currentConfig == null)
            {
                CalculatorGlobalConfiguration configToCreate = new()
                {
                    Id = "Configuration1",
                    StartDate = DateTime.Now,
                    EndDate = DateTime.Now,
                    PeopleNumber = 0,
                    NumLaborDaysInMonth = 0,
                    AdditionalGlobalIncrease = 0,
                    ProfitGreenClientAAA = 33,
                    ProfitGreenClientAA = 28,
                    ProfitGreenPartner = 21,
                    ProfitYellowClientAAA = 28,
                    ProfitYellowClientAA = 22,
                    ProfitYellowPartner = 17,
                    MinimumGlobalProfit = 20
                };
                _unitOfWork.CalculatorGlobalConfiguration.Add(configToCreate);
                _unitOfWork.Save();
            }
            Collection<CalculatorCostCenterIncreaseConfigurationVM> costCenterWithIncreaseList = new Collection<CalculatorCostCenterIncreaseConfigurationVM>();
            foreach (var costCenter in costCenterList)
            {
                var costCenterIncreaseFromDB = _unitOfWork.CalculatorCostCenterIncreaseConfiguration.GetFirstOrDefault(x =>
                x.CostCenterId == costCenter.CostCenterId);
                if (costCenterIncreaseFromDB != null)
                {
                    var description = _unitOfWork.CenterOfCosts.GetFirstOrDefault(x => x.CostCenterId == costCenterIncreaseFromDB.CostCenterId);
                    CalculatorCostCenterIncreaseConfigurationVM costCenterIncrease = new()
                    {
                        CostCenterId = costCenterIncreaseFromDB.CostCenterId,
                        Description = description.Description,
                        Increase = costCenterIncreaseFromDB.Increase, 
                        CompanyId = costCenter.CompanyId
                    };
                    costCenterWithIncreaseList.Add(costCenterIncrease);
                }
                else
                {
                    var description = _unitOfWork.CenterOfCosts.GetFirstOrDefault(x => x.CostCenterCode == costCenter.CostCenterCode);
                    CalculatorCostCenterIncreaseConfigurationVM costCenterIncrease = new()
                    {
                        CostCenterId = costCenter.CostCenterId,
                        Description = description.Description,
                        Increase = 0, 
                        CompanyId = costCenter.CompanyId
                    };
                    costCenterWithIncreaseList.Add(costCenterIncrease);
                }
            }

            CalculatorGlobalConfigurationVM cvm = new()
            {
                CalculatorGlobalConfiguration = currentConfig,
                CalculatorCostCenterIncreaseConfigurationVM = costCenterWithIncreaseList
            };
            return View("Index", cvm);
        }

        //POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SaveGlobalConfiguration(CalculatorGlobalConfigurationVM obj)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var claimsIdentity = (ClaimsIdentity)User.Identity;
                    var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

                    if (obj.CalculatorCostCenterIncreaseConfigurationVM != null)
                    {
                        foreach (var costCenterIncrease in obj.CalculatorCostCenterIncreaseConfigurationVM)
                        {
                            var existingCostCenterFormDB = _unitOfWork.CalculatorCostCenterIncreaseConfiguration.GetFirstOrDefault(x =>
                            x.CostCenterId == costCenterIncrease.CostCenterId);
                            if (existingCostCenterFormDB == null)
                            {
                                CalculatorCostCenterIncreaseConfiguration costCenterIncreaseConfigToSave = new()
                                {
                                    CostCenterId = (int)costCenterIncrease.CostCenterId,
                                    Increase = costCenterIncrease.Increase,
                                    IdUserUpdatedBy = claim.Value,
                                    DateLastUpdate = DateTime.Now
                                };
                                _unitOfWork.CalculatorCostCenterIncreaseConfiguration.Add(costCenterIncreaseConfigToSave);
                            }
                            else
                            {
                                existingCostCenterFormDB.Increase = costCenterIncrease.Increase;
                                existingCostCenterFormDB.IdUserUpdatedBy = claim.Value;
                                existingCostCenterFormDB.DateLastUpdate = DateTime.Now;
                                _unitOfWork.CalculatorCostCenterIncreaseConfiguration.Update(existingCostCenterFormDB);
                            }
                            _unitOfWork.Save();
                        }
                    }
                    var globalConfig = _unitOfWork.CalculatorGlobalConfiguration.GetFirstOrDefault(x => x.Id == "Configuration1");
                    if (globalConfig != null)
                    {
                        globalConfig.StartDate = obj.CalculatorGlobalConfiguration.StartDate;
                        globalConfig.EndDate = obj.CalculatorGlobalConfiguration.EndDate;
                        globalConfig.PeopleNumber = obj.CalculatorGlobalConfiguration.PeopleNumber;
                        globalConfig.NumLaborDaysInMonth = obj.CalculatorGlobalConfiguration.NumLaborDaysInMonth;
                        globalConfig.AdditionalGlobalIncrease = obj.CalculatorGlobalConfiguration.AdditionalGlobalIncrease;
                        globalConfig.ProfitGreenClientAAA = obj.CalculatorGlobalConfiguration.ProfitGreenClientAAA;
                        globalConfig.ProfitGreenClientAA = obj.CalculatorGlobalConfiguration.ProfitGreenClientAA;
                        globalConfig.ProfitGreenPartner = obj.CalculatorGlobalConfiguration.ProfitGreenPartner;
                        globalConfig.ProfitYellowClientAAA = obj.CalculatorGlobalConfiguration.ProfitYellowClientAAA;
                        globalConfig.ProfitYellowClientAA = obj.CalculatorGlobalConfiguration.ProfitYellowClientAA;
                        globalConfig.ProfitYellowPartner = obj.CalculatorGlobalConfiguration.ProfitYellowPartner;
                        globalConfig.MinimumGlobalProfit = obj.CalculatorGlobalConfiguration.MinimumGlobalProfit;
                        _unitOfWork.CalculatorGlobalConfiguration.Update(globalConfig);
                        _unitOfWork.Save();
                    }

                    TempData["success"] = "¡Los cambios fueron guardados con Éxito!";
                    return RedirectToAction("Index", "Calculator");
                }
                catch (Exception e)
                {
                    return BadRequest(e.Message);
                }
            }
            return View("Index", obj);
        }
    }
}
