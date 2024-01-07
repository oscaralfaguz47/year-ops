using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.Models;
using OceansApp.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.ObjectModel;
using System.Security.Claims;
using OceansApp.Models.ViewModels.ConsultantRolesQualityLevels;

namespace FinancialCalculatorWeb.Areas.Finances.Controllers
{
    [Area("Finances")]
    [RequireTwoFactorEnabled]
    [Authorize(Policy = "AccessToFinancialCalculatorConfig")]
    public class CalculatorGlobalConfigurationController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuthorizationService _authorizationService;
        public CalculatorGlobalConfigurationController(IUnitOfWork unitOrWork, IAuthorizationService authorizationService)
        {
            _unitOfWork = unitOrWork;
            _authorizationService = authorizationService;
        }

        public IActionResult Index()
        {
            IEnumerable<CostCenter> costCenterList = (IEnumerable<CostCenter>)_unitOfWork.CenterOfCosts.GetCostCenterOfExpenses();
            List<ConsultantRole> consultantRolesList = (List<ConsultantRole>)_unitOfWork.ConsultantRole.GetAll();
            List<ConsultantQualityLevel> consultantQualityLevelsList = (List<ConsultantQualityLevel>)_unitOfWork.ConsultantQualityLevel.GetAll();
            List<ConsultantSeniority> consultantSenioritisList = (List<ConsultantSeniority>)_unitOfWork.ConsultantSeniority.GetAll();
            List<GetConsultantRolesQualityLevelsVM> consultantRolesQualityLevelsList = (List<GetConsultantRolesQualityLevelsVM>)_unitOfWork.ConsultantRoleQualityLevel.GetConsultantRoleQualityLevelsList();
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
                CalculatorCostCenterIncreaseConfigurationVM = costCenterWithIncreaseList,
                ConsultantRolesQualityLevels = consultantRolesQualityLevelsList,
                ConsultantRolesList = consultantRolesList,
                ConsultantQualityLevelsList = consultantQualityLevelsList,
                ConsultantSenioritisList = consultantSenioritisList
            };
            return View("Index", cvm);
        }

        //POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveGlobalConfigurationAsync(CalculatorGlobalConfigurationVM obj)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var claimsIdentity = (ClaimsIdentity)User.Identity;
                    var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                    var costaRicaTime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, "Central America Standard Time");
                    int userIsAllowedAdvancedConfig = 0;
                    var advancedConfigEnabled = await _authorizationService.AuthorizeAsync(User, "AccessToFinancialCalculatorAdvancedConfig");
                    if (advancedConfigEnabled.Succeeded)
                    {
                        userIsAllowedAdvancedConfig++;
                    }

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
                                    DateLastUpdate = costaRicaTime
                                };
                                _unitOfWork.CalculatorCostCenterIncreaseConfiguration.Add(costCenterIncreaseConfigToSave);
                            }
                            else
                            {
                                existingCostCenterFormDB.Increase = costCenterIncrease.Increase;
                                existingCostCenterFormDB.IdUserUpdatedBy = claim.Value;
                                existingCostCenterFormDB.DateLastUpdate = costaRicaTime;
                                _unitOfWork.CalculatorCostCenterIncreaseConfiguration.Update(existingCostCenterFormDB);
                            }
                            _unitOfWork.Save();
                        }
                    }
                    foreach (var consultantQualityRole in obj.ConsultantRolesQualityLevels)
                    {
                        var existingCQFromDB = _unitOfWork.ConsultantRoleQualityLevel.GetFirstOrDefault(x =>
                        x.ConsultantRoleId == consultantQualityRole.ConsultantRoleId
                        && x.ConsultantQualityLevelId == consultantQualityRole.ConsultantQualityLevelId
                        && x.ConsultantSeniorityId == consultantQualityRole.ConsultantSeniorityId);
                        if (existingCQFromDB == null)
                        {
                            ConsultantRolesQualityLevels consultantQRToSave = new()
                            {
                                ConsultantRoleId = consultantQualityRole.ConsultantRoleId,
                                ConsultantQualityLevelId = consultantQualityRole.ConsultantQualityLevelId,
                                ConsultantSeniorityId = (int)consultantQualityRole.ConsultantSeniorityId,
                                ClientRateMaximumAmount = consultantQualityRole.ClientRateMaximumAmount,
                                ConsultantMaximumAmount = consultantQualityRole.ConsultantMaximumAmount,
                                UpdatedBy = claim.Value,
                                UpdatedDate = costaRicaTime
                            };
                            _unitOfWork.ConsultantRoleQualityLevel.Add(consultantQRToSave);
                        }
                        else
                        {
                            if (existingCQFromDB.ConsultantRoleId == consultantQualityRole.ConsultantRoleId &&
                                 existingCQFromDB.ConsultantQualityLevelId == consultantQualityRole.ConsultantQualityLevelId &&
                                 existingCQFromDB.ConsultantSeniorityId == consultantQualityRole.ConsultantSeniorityId &&
                                 existingCQFromDB.ClientRateMaximumAmount == consultantQualityRole.ClientRateMaximumAmount &&
                                 existingCQFromDB.ConsultantMaximumAmount == consultantQualityRole.ConsultantMaximumAmount)
                            {

                            }
                            else
                            {
                                existingCQFromDB.ConsultantRoleId = consultantQualityRole.ConsultantRoleId;
                                existingCQFromDB.ConsultantQualityLevelId = consultantQualityRole.ConsultantQualityLevelId;
                                existingCQFromDB.ConsultantSeniorityId = (int)consultantQualityRole.ConsultantSeniorityId;
                                existingCQFromDB.ClientRateMaximumAmount = consultantQualityRole.ClientRateMaximumAmount;
                                existingCQFromDB.ConsultantMaximumAmount = consultantQualityRole.ConsultantMaximumAmount;
                                existingCQFromDB.UpdatedBy = claim.Value;
                                existingCQFromDB.UpdatedDate = costaRicaTime;
                                _unitOfWork.ConsultantRoleQualityLevel.Update(existingCQFromDB);
                            }
                        }
                        _unitOfWork.Save();
                    }
                    var globalConfig = _unitOfWork.CalculatorGlobalConfiguration.GetFirstOrDefault(x => x.Id == "Configuration1");
                    if (globalConfig != null)
                    {
                        globalConfig.StartDate = obj.CalculatorGlobalConfiguration.StartDate;
                        globalConfig.EndDate = obj.CalculatorGlobalConfiguration.EndDate;
                        globalConfig.PeopleNumber = obj.CalculatorGlobalConfiguration.PeopleNumber;
                        globalConfig.NumLaborDaysInMonth = obj.CalculatorGlobalConfiguration.NumLaborDaysInMonth;
                        globalConfig.AdditionalGlobalIncrease = obj.CalculatorGlobalConfiguration.AdditionalGlobalIncrease;
                        if (userIsAllowedAdvancedConfig > 0)
                        {
                            globalConfig.ProfitGreenClientAAA = obj.CalculatorGlobalConfiguration.ProfitGreenClientAAA;
                            globalConfig.ProfitGreenClientAA = obj.CalculatorGlobalConfiguration.ProfitGreenClientAA;
                            globalConfig.ProfitGreenPartner = obj.CalculatorGlobalConfiguration.ProfitGreenPartner;
                            globalConfig.ProfitYellowClientAAA = obj.CalculatorGlobalConfiguration.ProfitYellowClientAAA;
                            globalConfig.ProfitYellowClientAA = obj.CalculatorGlobalConfiguration.ProfitYellowClientAA;
                            globalConfig.ProfitYellowPartner = obj.CalculatorGlobalConfiguration.ProfitYellowPartner;
                            globalConfig.MinimumGlobalProfit = obj.CalculatorGlobalConfiguration.MinimumGlobalProfit;
                        }
                        _unitOfWork.CalculatorGlobalConfiguration.Update(globalConfig);
                        _unitOfWork.Save();
                    }

                    TempData["success"] = "¡Los cambios fueron guardados con Éxito!";
                    return RedirectToAction("Index", "CalculatorGlobalConfiguration");
                }
                catch (Exception e)
                {
                    return BadRequest(e.Message);
                }
            }
            return View("Index", obj);
        }

     
        //GET
        //[HttpGet]
        //public async Task<IEnumerable<GetCareersPrincipalDataLandingViewModel>> GetCareersPrincipalDataEsp()
        //{
        //    var data = await _context.CareersPagesPrincipal.ToListAsync();
        //    return data.Select(d => new GetCareersPrincipalDataLandingViewModel
        //    {
        //        Title = d.TitleEsp,
        //        Intro = d.IntroEsp,
        //    });
        //}
    }
}
