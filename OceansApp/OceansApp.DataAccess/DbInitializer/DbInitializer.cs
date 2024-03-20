using OceansApp.Models.Models;
using OceansApp.Utility;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using OceansApp.DataAccess.Data;
using OceansApp.Utility.ConstantData.Claims.AdminCenter;
using OceansApp.Utility.ConstantData.Claims.Finances;
using OceansApp.Utility.ConstantData.Claims.General;
using OceansApp.Utility.ConstantData.Claims.Hours_TrackingTool;
using OceansApp.Utility.ConstantData.Claims.AccountManagement;

namespace OceansApp.DataAccess.DbInitializer
{
    public class DbInitializer : IDbInitializer
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _db;
        public IConfiguration _config { get; }
        public DbInitializer(
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext db,
            IConfiguration config)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _db = db;
            _config = config;
        }

        public void Initialize()
        {
            bool isThereNewMigrationToUpdate = true; // False if no migration updates in the DB are needed
            if (isThereNewMigrationToUpdate)
            {
                //Migrations if they are not applied
                try
                {
                    if (_db.Database.GetPendingMigrations().Count() > 0)
                    {
                        _db.Database.Migrate();
                    }
                }
                catch (Exception ex)
                {

                }
            }

            bool createDefaultDataToDatabase = true; // False if no updates in the DB are needed

            if (createDefaultDataToDatabase)
            {
                //-----------------  ROLES  --------------------------------

                List<IdentityRole> rolesList = new List<IdentityRole>();
                rolesList.Add(new IdentityRole() { Name = SD.Role_User_Master });
                rolesList.Add(new IdentityRole() { Name = SD.Role_User_Admin });
                rolesList.Add(new IdentityRole() { Name = SD.Role_User_Simple });
                rolesList.Add(new IdentityRole() { Name = SD.Role_User_Computer_Consultant });

                foreach (var role in rolesList)
                {
                    if (_roleManager.FindByNameAsync(role.Name).Result == null)
                    {
                        _roleManager.CreateAsync(new IdentityRole(role.Name)).GetAwaiter().GetResult();
                    }
                }

                //-----------------  USER CATEGORIES  --------------------------------

                List<ApplicationUserCategory> userCategoriesList = new List<ApplicationUserCategory>();
                userCategoriesList.Add(new ApplicationUserCategory() { Name = "Administrative" });
                userCategoriesList.Add(new ApplicationUserCategory() { Name = "Consultant" });
                userCategoriesList.Add(new ApplicationUserCategory() { Name = "External User" });
                foreach (var userCategory in userCategoriesList)
                {
                    if (_db.UserCategories.FirstOrDefault(x => x.Name == userCategory.Name) == null)
                    {
                        _db.UserCategories.Add(userCategory);
                    }
                    _db.SaveChanges();
                }

                //-----------------  CREATE DEFAULT USER  --------------------------------

                if (!_roleManager.RoleExistsAsync(SD.Role_User_Master).GetAwaiter().GetResult())
                {
                    //If Roles are not created, then we will create Master user as well
                    _userManager.CreateAsync(new ApplicationUser
                    {
                        UserName = Environment.GetEnvironmentVariable(_config["MasterUserEmail"]),
                        Email = Environment.GetEnvironmentVariable(_config["MasterUserEmail"]),
                        Name = _config["MasterUserName"],
                        LastName = _config["MasterUserLastName"],
                        IsActive = true,
                        DeactivationDate = null
                    }, Environment.GetEnvironmentVariable(_config["MasterUserPass_ENV"])).GetAwaiter().GetResult();
                    ApplicationUser user = _db.AspNetUsers.FirstOrDefault(x => x.Email == Environment.GetEnvironmentVariable(_config["MasterUserEmail"]));

                    _userManager.AddToRoleAsync(user, SD.Role_User_Master).GetAwaiter().GetResult();
                }

                //-----------------  CONSULTANT BENEFITS  --------------------------------

                List<ConsultantBenefit> consultantBenefitList = new List<ConsultantBenefit>();
                consultantBenefitList.Add(new ConsultantBenefit() { Name = "Balance Program", Amount = 750, BenefitPeriod = "Annual" });
                consultantBenefitList.Add(new ConsultantBenefit() { Name = "Bonusly", Amount = 5000, BenefitPeriod = "Undefined" });
                consultantBenefitList.Add(new ConsultantBenefit() { Name = "Oceans Challenge", Amount = 250, BenefitPeriod = "Annual" });

                foreach (var benefit in consultantBenefitList)
                {
                    var existingBenefit = _db.CONSULTANT_BENEFITS.FirstOrDefault(x => x.Name == benefit.Name);
                    if (existingBenefit == null)
                    {
                        ConsultantBenefit conBenefit = new()
                        {
                            Name = benefit.Name,
                            Amount = benefit.Amount,
                            BenefitPeriod = benefit.BenefitPeriod
                        };
                        _db.CONSULTANT_BENEFITS.Add(conBenefit);
                    }
                }
                _db.SaveChanges();

                //-----------------  CONSULTANT BENEFITS CATEGORIES  --------------------------------

                List<ConsultantBenefitCategory> consultantBenefitCategoriesList = new List<ConsultantBenefitCategory>();
                var balanceProgramBenefit = _db.CONSULTANT_BENEFITS.FirstOrDefault(x => x.Name == "Balance Program");
                var bonuslyBenefit = _db.CONSULTANT_BENEFITS.FirstOrDefault(x => x.Name == "Bonusly");
                var oceansChallengeBenefit = _db.CONSULTANT_BENEFITS.FirstOrDefault(x => x.Name == "Oceans Challenge");

                consultantBenefitCategoriesList.Add(new ConsultantBenefitCategory() { Name = "Expert Boost ($250) (2500 Bonus.ly XP)", BenefitId = balanceProgramBenefit.BenefitId });
                consultantBenefitCategoriesList.Add(new ConsultantBenefitCategory() { Name = "Wellness Coverage ($750)", BenefitId = balanceProgramBenefit.BenefitId });

                consultantBenefitCategoriesList.Add(new ConsultantBenefitCategory() { Name = " Curiosity Stream 1 year ($25)", BenefitId = bonuslyBenefit.BenefitId });
                consultantBenefitCategoriesList.Add(new ConsultantBenefitCategory() { Name = "A new gaming console ($500)", BenefitId = bonuslyBenefit.BenefitId });
                consultantBenefitCategoriesList.Add(new ConsultantBenefitCategory() { Name = "Adventure tickets ($100)", BenefitId = bonuslyBenefit.BenefitId });
                consultantBenefitCategoriesList.Add(new ConsultantBenefitCategory() { Name = "Buy a book! ($25)", BenefitId = bonuslyBenefit.BenefitId });
                consultantBenefitCategoriesList.Add(new ConsultantBenefitCategory() { Name = "Ergonomics ($150)", BenefitId = bonuslyBenefit.BenefitId });
                consultantBenefitCategoriesList.Add(new ConsultantBenefitCategory() { Name = "Gamers ($200)", BenefitId = bonuslyBenefit.BenefitId });
                consultantBenefitCategoriesList.Add(new ConsultantBenefitCategory() { Name = "Hotel or plane tickets ($240/$480/$750)", BenefitId = bonuslyBenefit.BenefitId });
                consultantBenefitCategoriesList.Add(new ConsultantBenefitCategory() { Name = "Just Cash Out ($50/$100/$200)", BenefitId = bonuslyBenefit.BenefitId });
                consultantBenefitCategoriesList.Add(new ConsultantBenefitCategory() { Name = "Lodgings ($100) ", BenefitId = bonuslyBenefit.BenefitId });
                consultantBenefitCategoriesList.Add(new ConsultantBenefitCategory() { Name = "Movie Night ($30)", BenefitId = bonuslyBenefit.BenefitId });
                consultantBenefitCategoriesList.Add(new ConsultantBenefitCategory() { Name = "Music Lovers! ($60)", BenefitId = bonuslyBenefit.BenefitId });
                consultantBenefitCategoriesList.Add(new ConsultantBenefitCategory() { Name = "N. Fitness Freaks ($120)", BenefitId = bonuslyBenefit.BenefitId });
                consultantBenefitCategoriesList.Add(new ConsultantBenefitCategory() { Name = "Nintendo Switch ONLINE 1 year ($40)", BenefitId = bonuslyBenefit.BenefitId });
                consultantBenefitCategoriesList.Add(new ConsultantBenefitCategory() { Name = "Out for dinner ($80)", BenefitId = bonuslyBenefit.BenefitId });
                consultantBenefitCategoriesList.Add(new ConsultantBenefitCategory() { Name = "Personal Care ($35/$70/$140)", BenefitId = bonuslyBenefit.BenefitId });
                consultantBenefitCategoriesList.Add(new ConsultantBenefitCategory() { Name = "PlayStation Plus ($60)", BenefitId = bonuslyBenefit.BenefitId });
                consultantBenefitCategoriesList.Add(new ConsultantBenefitCategory() { Name = "Streaming Subscriptions ($20)", BenefitId = bonuslyBenefit.BenefitId });
                consultantBenefitCategoriesList.Add(new ConsultantBenefitCategory() { Name = "Tech gadgets I ($30)", BenefitId = bonuslyBenefit.BenefitId });
                consultantBenefitCategoriesList.Add(new ConsultantBenefitCategory() { Name = "Tech gadgets II ($140)", BenefitId = bonuslyBenefit.BenefitId });
                consultantBenefitCategoriesList.Add(new ConsultantBenefitCategory() { Name = "Tech gadgets III ($300)", BenefitId = bonuslyBenefit.BenefitId });
                consultantBenefitCategoriesList.Add(new ConsultantBenefitCategory() { Name = "UberEats voucher ($25)", BenefitId = bonuslyBenefit.BenefitId });

                consultantBenefitCategoriesList.Add(new ConsultantBenefitCategory() { Name = "Courses (in person/online)", BenefitId = oceansChallengeBenefit.BenefitId });
                consultantBenefitCategoriesList.Add(new ConsultantBenefitCategory() { Name = "Licenses for learning tools and work support", BenefitId = oceansChallengeBenefit.BenefitId });
                consultantBenefitCategoriesList.Add(new ConsultantBenefitCategory() { Name = "Universities Enrollment", BenefitId = oceansChallengeBenefit.BenefitId });
                consultantBenefitCategoriesList.Add(new ConsultantBenefitCategory() { Name = "Certificates", BenefitId = oceansChallengeBenefit.BenefitId });

                foreach (var category in consultantBenefitCategoriesList)
                {
                    var existingCategory = _db.CONSULTANT_BENEFIT_CATEGORIES.FirstOrDefault(x => x.Name == category.Name);
                    if (existingCategory == null)
                    {
                        ConsultantBenefitCategory conBenefitCategory = new()
                        {
                            Name = category.Name,
                            BenefitId = category.BenefitId
                        };
                        _db.CONSULTANT_BENEFIT_CATEGORIES.Add(conBenefitCategory);
                    }
                }
                _db.SaveChanges();

                //-----------------  CONSULTANT BENEFITS COMPANIES  --------------------------------

                List<ConsultantBenefitCompany> consultantBenefitCompaniesList = new List<ConsultantBenefitCompany>();
                var peopleAndCultureCostCenterOCE = _db.COST_CENTER.FirstOrDefault(x => x.CostCenterCode == "10-02-04" && x.CompanyId == "OCE");
                var peopleAndCultureCostCenterLLC = _db.COST_CENTER.FirstOrDefault(x => x.CostCenterCode == "10-02-04" && x.CompanyId == "LLC");
                var accountingAccountReservaBalanceProgramOCE = _db.ACCOUNTING_ACCOUNT.FirstOrDefault(x => x.AccountingAccountCode == "3-02-01-000-000" && x.CompanyId == "OCE");
                var accountingAccountReservaBonuslyOCE = _db.ACCOUNTING_ACCOUNT.FirstOrDefault(x => x.AccountingAccountCode == "3-02-02-000-000" && x.CompanyId == "OCE");
                var accountingAccountOceansChallengeOCE = _db.ACCOUNTING_ACCOUNT.FirstOrDefault(x => x.AccountingAccountCode == "6-01-03-005-000" && x.CompanyId == "OCE");
                var accountingAccountAdminExpensesLLC = _db.ACCOUNTING_ACCOUNT.FirstOrDefault(x => x.AccountingAccountCode == "6-01-04-013-0000" && x.CompanyId == "LLC");

                //OCE
                consultantBenefitCompaniesList.Add(new ConsultantBenefitCompany()
                {
                    CompanyId = "OCE",
                    CostCenterId = peopleAndCultureCostCenterOCE.CostCenterId,
                    AccountingAccountId = accountingAccountReservaBalanceProgramOCE.AccountingAccountId,
                    BenefitId = balanceProgramBenefit.BenefitId
                });
                consultantBenefitCompaniesList.Add(new ConsultantBenefitCompany()
                {
                    CompanyId = "OCE",
                    CostCenterId = peopleAndCultureCostCenterOCE.CostCenterId,
                    AccountingAccountId = accountingAccountReservaBonuslyOCE.AccountingAccountId,
                    BenefitId = bonuslyBenefit.BenefitId
                });
                consultantBenefitCompaniesList.Add(new ConsultantBenefitCompany()
                {
                    CompanyId = "OCE",
                    CostCenterId = peopleAndCultureCostCenterOCE.CostCenterId,
                    AccountingAccountId = accountingAccountOceansChallengeOCE.AccountingAccountId,
                    BenefitId = oceansChallengeBenefit.BenefitId
                });
                //LLC
                consultantBenefitCompaniesList.Add(new ConsultantBenefitCompany()
                {
                    CompanyId = "LLC",
                    CostCenterId = peopleAndCultureCostCenterLLC.CostCenterId,
                    AccountingAccountId = accountingAccountAdminExpensesLLC.AccountingAccountId,
                    BenefitId = balanceProgramBenefit.BenefitId
                });
                consultantBenefitCompaniesList.Add(new ConsultantBenefitCompany()
                {
                    CompanyId = "LLC",
                    CostCenterId = peopleAndCultureCostCenterLLC.CostCenterId,
                    AccountingAccountId = accountingAccountAdminExpensesLLC.AccountingAccountId,
                    BenefitId = bonuslyBenefit.BenefitId
                });
                consultantBenefitCompaniesList.Add(new ConsultantBenefitCompany()
                {
                    CompanyId = "LLC",
                    CostCenterId = peopleAndCultureCostCenterLLC.CostCenterId,
                    AccountingAccountId = accountingAccountAdminExpensesLLC.AccountingAccountId,
                    BenefitId = oceansChallengeBenefit.BenefitId
                });

                foreach (var benefitCompany in consultantBenefitCompaniesList)
                {
                    var existingBenefitCompany = _db.CONSULTANT_BENEFIT_COMPANIES.FirstOrDefault(x => x.CompanyId == benefitCompany.CompanyId && 
                    x.CostCenterId == benefitCompany.CostCenterId && x.AccountingAccountId == benefitCompany.AccountingAccountId);
                    if (existingBenefitCompany == null)
                    {
                        ConsultantBenefitCompany conBenefitCompany = new()
                        {
                            CompanyId  = benefitCompany.CompanyId,
                            CostCenterId = benefitCompany.CostCenterId,
                            AccountingAccountId = benefitCompany.AccountingAccountId,
                            BenefitId = benefitCompany.BenefitId
                        };
                        _db.CONSULTANT_BENEFIT_COMPANIES.Add(conBenefitCompany);
                    }
                }
                _db.SaveChanges();

                //-----------------  NOTIFICATIONS MEDIA  --------------------------------

                List<NotificationMedia> notificatinMediaList = new List<NotificationMedia>();
                notificatinMediaList.Add(new NotificationMedia() { Name = "Email" });
                notificatinMediaList.Add(new NotificationMedia() { Name = "Slack" });

                foreach (var notMedia in notificatinMediaList)
                {
                    var existingMedia = _db.NOTIFICATION_MEDIA.FirstOrDefault(x => x.Name == notMedia.Name);
                    if (existingMedia == null)
                    {
                        NotificationMedia notificationMedia = new()
                        {
                            Name = notMedia.Name
                        };
                        _db.NOTIFICATION_MEDIA.Add(notificationMedia);
                    }
                }
                _db.SaveChanges();

                //-----------------  NOTIFICATION STATUS  --------------------------------

                List<NotificationStatus> notificatinStatusList = new List<NotificationStatus>();
                notificatinStatusList.Add(new NotificationStatus() { Name = "Enviando" });
                notificatinStatusList.Add(new NotificationStatus() { Name = "Enviado" });
                notificatinStatusList.Add(new NotificationStatus() { Name = "No enviado" });
                notificatinStatusList.Add(new NotificationStatus() { Name = "Envío fallido" });
                foreach (var notStatus in notificatinStatusList)
                {
                    var existingNS = _db.NOTIFICATION_STATUS.FirstOrDefault(x => x.Name == notStatus.Name);
                    if (existingNS == null)
                    {
                        NotificationStatus notificationStatus = new()
                        {
                            Name = notStatus.Name
                        };
                        _db.NOTIFICATION_STATUS.Add(notificationStatus);
                    }
                }
                _db.SaveChanges();

                //-----------------  NOTIFICATION TYPES  --------------------------------

                List<NotificationType> notificationTypeList = new List<NotificationType>();
                notificationTypeList.Add(new NotificationType() { Name = "Cuentas por cobrar" });
                notificationTypeList.Add(new NotificationType() { Name = "Create new Consultant" });
                foreach (var notType in notificationTypeList)
                {
                    if (_db.NOTIFICATION_TYPES.FirstOrDefault(x => x.Name == notType.Name) == null)
                    {
                        NotificationType notificationType = new()
                        {
                            Name = notType.Name
                        };
                        _db.NOTIFICATION_TYPES.Add(notificationType);
                    }
                }
                _db.SaveChanges();

                //-----------------  CONSULTANT POSITIONS  --------------------------------

                List<ConsultantPosition> positionsList = new List<ConsultantPosition>();
                positionsList.Add(new ConsultantPosition() { Name = "Success Manager", IsAdministrative = true });
                positionsList.Add(new ConsultantPosition() { Name = "CEO", IsAdministrative = true });
                positionsList.Add(new ConsultantPosition() { Name = "CFO", IsAdministrative = true });
                positionsList.Add(new ConsultantPosition() { Name = "Recruiting Manager", IsAdministrative = true });
                positionsList.Add(new ConsultantPosition() { Name = "Strategy Director", IsAdministrative = true });
                positionsList.Add(new ConsultantPosition() { Name = "Recruiter", IsAdministrative = true });
                positionsList.Add(new ConsultantPosition() { Name = "Marketing Manager", IsAdministrative = true });
                positionsList.Add(new ConsultantPosition() { Name = "People and Culture", IsAdministrative = true });
                positionsList.Add(new ConsultantPosition() { Name = "Gifts Coordinator", IsAdministrative = true });
                positionsList.Add(new ConsultantPosition() { Name = "Junior Sales Executive", IsAdministrative = true });
                positionsList.Add(new ConsultantPosition() { Name = "Sales Executive", IsAdministrative = true });
                positionsList.Add(new ConsultantPosition() { Name = "Payment Assistant", IsAdministrative = true });
                positionsList.Add(new ConsultantPosition() { Name = "Financial Assistant", IsAdministrative = true });
                positionsList.Add(new ConsultantPosition() { Name = "Full Stack Developer IT Support", IsAdministrative = true });

                positionsList.Add(new ConsultantPosition() { Name = "Senior Developer", IsAdministrative = false });
                positionsList.Add(new ConsultantPosition() { Name = "Full Stack Developer", IsAdministrative = false });
                positionsList.Add(new ConsultantPosition() { Name = "Data Engineer", IsAdministrative = false });
                positionsList.Add(new ConsultantPosition() { Name = "Senior QA Engineer", IsAdministrative = false });
                positionsList.Add(new ConsultantPosition() { Name = "Mid Developer", IsAdministrative = false });
                positionsList.Add(new ConsultantPosition() { Name = "Project Manager", IsAdministrative = false });
                positionsList.Add(new ConsultantPosition() { Name = "Team Lead", IsAdministrative = false });
                positionsList.Add(new ConsultantPosition() { Name = "AWS Engineer", IsAdministrative = false });
                positionsList.Add(new ConsultantPosition() { Name = "DevOps Engineer", IsAdministrative = false });
                positionsList.Add(new ConsultantPosition() { Name = "SRE Developer", IsAdministrative = false });
                positionsList.Add(new ConsultantPosition() { Name = "Mobile Developer", IsAdministrative = false });
                positionsList.Add(new ConsultantPosition() { Name = "QA Lead", IsAdministrative = false });

                foreach (var conPosition in positionsList)
                {
                    if (_db.CONSULTANT_POSITIONS.FirstOrDefault(x => x.Name == conPosition.Name) == null)
                    {
                        ConsultantPosition consultantPosition = new()
                        {
                            Name = conPosition.Name,
                            IsAdministrative = conPosition.IsAdministrative
                        };
                        _db.CONSULTANT_POSITIONS.Add(consultantPosition);
                    }
                }
                _db.SaveChanges();

                //-----------------  DEFAULT CLIENT FOR ADMINISTRATIVE CONSULTANTS  --------------------------------

                if (_db.CLIENT.FirstOrDefault(x => x.Name == "Oceans Code Experts") == null)
                {
                    Client client = new()
                    {
                        Name = "Oceans Code Experts",
                        ClientCode = "OCEADMIN01",
                        Alias = "Oceans Code Experts",
                        AdmissionDate = DateTime.Now,
                        PaymentCondition = "ND",
                        Discount = 0,
                        IsActive = "S",
                        ClientCategory = "OCEADMIN",
                        CreationDate = DateTime.Now,
                        CompanyId = "OCE/LLC",
                        LatePaymentFee = 0,
                        AllowSentLatePaymentNotifications = false
                    };
                    _db.CLIENT.Add(client);
                    _db.SaveChanges();
                }

                //-----------------  PROJECT CONSULTANTS ASSIGNED HISTORY ACTIONS  --------------------------------

                if (_db.PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_ACTIONS.ToList().Count == 0)
                {
                    List<ProjectConsultantAssignedHistoryAction> actionsList = new();
                    actionsList.Add(new ProjectConsultantAssignedHistoryAction() { Name = "Consultant Assigned First Time" });
                    actionsList.Add(new ProjectConsultantAssignedHistoryAction() { Name = "Position Details updated" });
                    actionsList.Add(new ProjectConsultantAssignedHistoryAction() { Name = "Hourly Client Rate updated" });
                    actionsList.Add(new ProjectConsultantAssignedHistoryAction() { Name = "Monthly Client Rate updated" });
                    actionsList.Add(new ProjectConsultantAssignedHistoryAction() { Name = "Hourly Salary updated" });
                    actionsList.Add(new ProjectConsultantAssignedHistoryAction() { Name = "Monthly Salary updated" });
                    actionsList.Add(new ProjectConsultantAssignedHistoryAction() { Name = "Client pricing method updated (Monthly)" });
                    actionsList.Add(new ProjectConsultantAssignedHistoryAction() { Name = "Client pricing method updated (Hourly)" });
                    actionsList.Add(new ProjectConsultantAssignedHistoryAction() { Name = "Consultant pricing method updated (Monthly)" });
                    actionsList.Add(new ProjectConsultantAssignedHistoryAction() { Name = "Consultant pricing method updated (Hourly)" });
                    actionsList.Add(new ProjectConsultantAssignedHistoryAction() { Name = "Consultant Activated" });
                    actionsList.Add(new ProjectConsultantAssignedHistoryAction() { Name = "Consultant Deactivated" });
                    foreach (var action in actionsList)
                    {
                        ProjectConsultantAssignedHistoryAction actionToSave = new()
                        {
                            Name = action.Name
                        };
                        _db.PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_ACTIONS.Add(actionToSave);
                    }
                    _db.SaveChanges();
                }

                //-----------------  SYSTEM AREAS  --------------------------------

                List<SystemArea> systemAreasList = new List<SystemArea>();
                systemAreasList.Add(new SystemArea() { Name = "Admin Center" });
                systemAreasList.Add(new SystemArea() { Name = "Finanzas" });
                systemAreasList.Add(new SystemArea() { Name = "General" });
                systemAreasList.Add(new SystemArea() { Name = "Reporte de Horas" });
                systemAreasList.Add(new SystemArea() { Name = "Dashboard" });
                systemAreasList.Add(new SystemArea() { Name = "Mi Cuenta" });
                systemAreasList.Add(new SystemArea() { Name = "Account Management" });

                foreach (var area in systemAreasList)
                {
                    if (_db.SYSTEM_AREAS.FirstOrDefault(x => x.Name == area.Name) == null)
                    {
                        SystemArea sa = new()
                        {
                            Name = area.Name
                        };
                        _db.SYSTEM_AREAS.Add(sa);
                    }
                }
                _db.SaveChanges();

                //-----------------  SYSTEM SUB AREAS  --------------------------------

                List<SystemSubArea> systemSubAreasList = new List<SystemSubArea>();
                systemSubAreasList.Add(new SystemSubArea() { SystemAreaId = 1, Name = "Actualizar Datos desde Softland" });
                systemSubAreasList.Add(new SystemSubArea() { SystemAreaId = 1, Name = "Administración de Usuarios" });
                systemSubAreasList.Add(new SystemSubArea() { SystemAreaId = 1, Name = "Roles y Permisos de Usuarios" });
                systemSubAreasList.Add(new SystemSubArea() { SystemAreaId = 2, Name = "Cuentas Por Cobrar" });
                systemSubAreasList.Add(new SystemSubArea() { SystemAreaId = 2, Name = "Consultant Payment Debits & Credits" });
                systemSubAreasList.Add(new SystemSubArea() { SystemAreaId = 2, Name = "Calculadora Financiera" });
                systemSubAreasList.Add(new SystemSubArea() { SystemAreaId = 3, Name = "Consultants" });
                systemSubAreasList.Add(new SystemSubArea() { SystemAreaId = 3, Name = "Consultant Reimbursed Benefits" });
                systemSubAreasList.Add(new SystemSubArea() { SystemAreaId = 3, Name = "Holidays" });
                systemSubAreasList.Add(new SystemSubArea() { SystemAreaId = 4, Name = "Herramienta de seguimiento de horas" });
                systemSubAreasList.Add(new SystemSubArea() { SystemAreaId = 5, Name = "Dashboard" });
                systemSubAreasList.Add(new SystemSubArea() { SystemAreaId = 6, Name = "Mi Cuenta" });
                systemSubAreasList.Add(new SystemSubArea() { SystemAreaId = 7, Name = "Clients" });
                systemSubAreasList.Add(new SystemSubArea() { SystemAreaId = 7, Name = "Projects" });

                foreach (var subArea in systemSubAreasList)
                {
                    if (_db.SYSTEM_SUB_AREAS.FirstOrDefault(x => x.Name == subArea.Name) == null)
                    {
                        SystemSubArea ssa = new()
                        {
                            Name = subArea.Name,
                            SystemAreaId = subArea.SystemAreaId
                        };
                        _db.SYSTEM_SUB_AREAS.Add(ssa);
                    }
                }
                _db.SaveChanges();

                //-----------------  PAYMENT METHODS  --------------------------------

                List<PaymentMethod> paymentMethodsList = new List<PaymentMethod>();
                paymentMethodsList.Add(new PaymentMethod() { Name = "Bac Credomatic different from Panamá (Ameritransfer)", CompanyId = "OCE" });
                paymentMethodsList.Add(new PaymentMethod() { Name = "Other banks (International Transfer)", CompanyId = "OCE" });
                paymentMethodsList.Add(new PaymentMethod() { Name = "Payoneer", CompanyId = "OCE" });
                paymentMethodsList.Add(new PaymentMethod() { Name = "Banco General (Panamá)", CompanyId = "OCE" });
                paymentMethodsList.Add(new PaymentMethod() { Name = "Bac Credomatic (Panamá)", CompanyId = "OCE" });
                paymentMethodsList.Add(new PaymentMethod() { Name = "Mercury", CompanyId = "LLC" });
                paymentMethodsList.Add(new PaymentMethod() { Name = "Wise", CompanyId = "LLC" });

                foreach (var paymentMethod in paymentMethodsList)
                {
                    if (_db.PAYMENT_METHODS.FirstOrDefault(x => x.Name == paymentMethod.Name) == null)
                    {
                        PaymentMethod pm = new()
                        {
                            Name = paymentMethod.Name,
                            CompanyId = paymentMethod.CompanyId
                        };
                        _db.PAYMENT_METHODS.Add(pm);
                    }
                }
                _db.SaveChanges();

                //-----------------  TRANSACTION TYPES  --------------------------------

                List<TransactionType> transactionTypesList = new List<TransactionType>();
                transactionTypesList.Add(new TransactionType() { Name = "Debit" });
                transactionTypesList.Add(new TransactionType() { Name = "Credit" });

                foreach (var type in transactionTypesList)
                {
                    var existingType = _db.TRANSACTION_TYPES.FirstOrDefault(x => x.Name == type.Name);
                    if (existingType == null)
                    {
                        TransactionType transactionType = new()
                        {
                            Name = type.Name
                        };
                        _db.TRANSACTION_TYPES.Add(transactionType);
                    }
                }
                _db.SaveChanges();

                //-----------------  TRANSACTION STATUSES  --------------------------------

                List<TransactionStatus> transactionStatusesList = new List<TransactionStatus>();
                transactionStatusesList.Add(new TransactionStatus() { Name = "Waiting to be approved" });
                transactionStatusesList.Add(new TransactionStatus() { Name = "Approved" });
                transactionStatusesList.Add(new TransactionStatus() { Name = "Rejected" });
                transactionStatusesList.Add(new TransactionStatus() { Name = "Approved and sent" });
                transactionStatusesList.Add(new TransactionStatus() { Name = "Paid" });
                transactionStatusesList.Add(new TransactionStatus() { Name = "Accounted - Accounts Payable" });
                transactionStatusesList.Add(new TransactionStatus() { Name = "Done" });

                foreach (var status in transactionStatusesList)
                {
                    var existingStatus = _db.TRANSACTION_STATUSES.FirstOrDefault(x => x.Name == status.Name);
                    if (existingStatus == null)
                    {
                        TransactionStatus transactionStatus = new()
                        {
                            Name = status.Name
                        };
                        _db.TRANSACTION_STATUSES.Add(transactionStatus);
                    }
                }
                _db.SaveChanges();

                //-----------------  CLAIMS  --------------------------------

                List<ApplicationSystemClaim> systemClaimsList = new List<ApplicationSystemClaim>();

                //ADMIN CENTER
                var softlandSubAreaId = _db.SYSTEM_SUB_AREAS.FirstOrDefault(x => x.Name == "Actualizar Datos desde Softland");
                systemClaimsList.Add(new ApplicationSystemClaim()
                {
                    ClaimType = AdminCenterClaimsCD.Actualizar_Datos_Softland_ClaimType,
                    ClaimValue = AdminCenterClaimsCD.Actualizar_Datos_Softland_ClaimValue,
                    Description = "Acceso a poder actualizar los datos extraídos desde Softland",
                    SystemSubAreaId = softlandSubAreaId.SystemSubAreaId
                });

                var adminUserSubAreaId = _db.SYSTEM_SUB_AREAS.FirstOrDefault(x => x.Name == "Administración de Usuarios");
                systemClaimsList.Add(new ApplicationSystemClaim()
                {
                    ClaimType = AdminCenterClaimsCD.Administracion_Usuarios_ClaimType,
                    ClaimValue = AdminCenterClaimsCD.Administracion_Usuarios_ClaimValue,
                    Description = "Acceso a ver todos los usuarios del sistema",
                    SystemSubAreaId = adminUserSubAreaId.SystemSubAreaId
                });

                var userRolesPermissionsSubAreaId = _db.SYSTEM_SUB_AREAS.FirstOrDefault(x => x.Name == "Roles y Permisos de Usuarios");
                systemClaimsList.Add(new ApplicationSystemClaim()
                {
                    ClaimType = AdminCenterClaimsCD.Roles_Permisos_Usuarios_ClaimType,
                    ClaimValue = AdminCenterClaimsCD.Roles_Permisos_Usuarios_ClaimValue,
                    Description = "Acceso a ver y editar los roles y permisos de usuarios",
                    SystemSubAreaId = userRolesPermissionsSubAreaId.SystemSubAreaId
                });
                // NOTES FOR ADMIN CENTER PERMISSIONS:
                // Add every permission to the AnyOfPoliciesAdminCenterRequirementHandler

                //FINANCES
                var accountsReceivableSubAreaId = _db.SYSTEM_SUB_AREAS.FirstOrDefault(x => x.Name == "Cuentas Por Cobrar");
                systemClaimsList.Add(new ApplicationSystemClaim()
                {
                    ClaimType = FinancesClaimsCD.Accounts_Receivable_ClaimType,
                    ClaimValue = FinancesClaimsCD.Accounts_Receivable_ClaimValue,
                    Description = "Acceso a la sección de cuentas por cobrar",
                    SystemSubAreaId = accountsReceivableSubAreaId.SystemSubAreaId
                });

                var financialCalculatorSubAreaId = _db.SYSTEM_SUB_AREAS.FirstOrDefault(x => x.Name == "Calculadora Financiera");
                systemClaimsList.Add(new ApplicationSystemClaim()
                {
                    ClaimType = FinancesClaimsCD.Financial_Calculator_ClaimType,
                    ClaimValue = FinancesClaimsCD.Financial_Calculator_ClaimValue,
                    Description = "Acceso básico a la calculadora financiera",
                    SystemSubAreaId = financialCalculatorSubAreaId.SystemSubAreaId
                });
                systemClaimsList.Add(new ApplicationSystemClaim()
                {
                    ClaimType = FinancesClaimsCD.Financial_Calculator_BasicConfig_ClaimType,
                    ClaimValue = FinancesClaimsCD.Financial_Calculator_BasicConfig_ClaimValue,
                    Description = "Acceso a la configuración básica de la calculadora financiera",
                    SystemSubAreaId = financialCalculatorSubAreaId.SystemSubAreaId
                });
                systemClaimsList.Add(new ApplicationSystemClaim()
                {
                    ClaimType = FinancesClaimsCD.Financial_Calculator_AdvancedConfig_ClaimType,
                    ClaimValue = FinancesClaimsCD.Financial_Calculator_AdvancedConfig_ClaimValue,
                    Description = "Acceso avanzado en la configuración de la calculadora (editar los porcentages de utilidad, porcentages de riesgo, etc.)",
                    SystemSubAreaId = financialCalculatorSubAreaId.SystemSubAreaId
                });
                systemClaimsList.Add(new ApplicationSystemClaim()
                {
                    ClaimType = FinancesClaimsCD.Financial_Calculator_Profit_And_Details_ClaimType,
                    ClaimValue = FinancesClaimsCD.Financial_Calculator_Profit_And_Details_ClaimValue,
                    Description = "Acceso a ver las utilidades de los resultados de la calculadora financiera y a más detalles.",
                    SystemSubAreaId = financialCalculatorSubAreaId.SystemSubAreaId
                });
                systemClaimsList.Add(new ApplicationSystemClaim()
                {
                    ClaimType = FinancesClaimsCD.Financial_Calculator_Remove_Expenses_And_Costs_And_Edit_Vacations_ClaimType,
                    ClaimValue = FinancesClaimsCD.Financial_Calculator_Remove_Expenses_And_Costs_And_Edit_Vacations_ClaimValue,
                    Description = "Acceso a editar la opcion de vacaciones y remover gastos y costos para no ser tomados en cuenta en el calculo de la calculadora financiera.",
                    SystemSubAreaId = financialCalculatorSubAreaId.SystemSubAreaId
                });

                var paymentDebitsAndCreditsSubAreaId = _db.SYSTEM_SUB_AREAS.FirstOrDefault(x => x.Name == "Consultant Payment Debits & Credits");
                systemClaimsList.Add(new ApplicationSystemClaim()
                {
                    ClaimType = FinancesClaimsCD.Manage_Payment_Debits_Credits_ClaimType,
                    ClaimValue = FinancesClaimsCD.Manage_Payment_Debits_Credits_ClaimValue,
                    Description = "Have access to manage payment debits and credits of payments to consultants.",
                    SystemSubAreaId = paymentDebitsAndCreditsSubAreaId.SystemSubAreaId
                });

                //GENERAL - CONSULTANTS
                var consultantsSubAreaId = _db.SYSTEM_SUB_AREAS.FirstOrDefault(x => x.Name == "Consultants");
                systemClaimsList.Add(new ApplicationSystemClaim()
                {
                    ClaimType = ConsultantsClaimsCD.Consultants_Page_ClaimType,
                    ClaimValue = ConsultantsClaimsCD.Consultants_Page_ClaimValue,
                    Description = "Access to manage only Computer Consultants (Developers, QAs...)",
                    SystemSubAreaId = consultantsSubAreaId.SystemSubAreaId
                });
                systemClaimsList.Add(new ApplicationSystemClaim()
                {
                    ClaimType = ConsultantsClaimsCD.Manage_Administrative_Consultants_ClaimType,
                    ClaimValue = ConsultantsClaimsCD.Manage_Administrative_Consultants_ClaimValue,
                    Description = "Access to manage all consultants, including Administrative Consultants",
                    SystemSubAreaId = consultantsSubAreaId.SystemSubAreaId
                });

                //GENERAL - HOLIDAYS
                var holidaysSubAreaId = _db.SYSTEM_SUB_AREAS.FirstOrDefault(x => x.Name == "Holidays");
                systemClaimsList.Add(new ApplicationSystemClaim()
                {
                    ClaimType = HolidaysClaimsCD.Holidays_Page_ClaimType,
                    ClaimValue = HolidaysClaimsCD.Holidays_Page_ClaimValue,
                    Description = "Acceso básico para ver todos los holidays",
                    SystemSubAreaId = holidaysSubAreaId.SystemSubAreaId
                });

                //GENERAL - CONSULTANT REIMBURSED BENEFITS
                var consultantsBenefitsSubAreaId = _db.SYSTEM_SUB_AREAS.FirstOrDefault(x => x.Name == "Consultant Reimbursed Benefits");
                systemClaimsList.Add(new ApplicationSystemClaim()
                {
                    ClaimType = ConsultantReimbursedBenefitsClaimsCD.Manage_Consultant_Reimbursed_Benefits_ClaimType,
                    ClaimValue = ConsultantReimbursedBenefitsClaimsCD.Manage_Consultant_Reimbursed_Benefits_ClaimValue,
                    Description = "Access to manage the consultant reimbursed benefits to pay.",
                    SystemSubAreaId = consultantsBenefitsSubAreaId.SystemSubAreaId
                });


                //HOURS TRACKING TOOL
                var hoursTrackingToolSubAreaId = _db.SYSTEM_SUB_AREAS.FirstOrDefault(x => x.Name == "Herramienta de seguimiento de horas");
                systemClaimsList.Add(new ApplicationSystemClaim()
                {
                    ClaimType = HoursTrackingToolClaimsCD.Hours_Tracking_Tool_ClaimType,
                    ClaimValue = HoursTrackingToolClaimsCD.Hours_Tracking_Tool_ClaimValue,
                    Description = "Acceso a reportar horas en el tracking tool",
                    SystemSubAreaId = hoursTrackingToolSubAreaId.SystemSubAreaId
                });

                //PROJECT MANAGEMENT - CLIENTS
                var clientsSubAreaId = _db.SYSTEM_SUB_AREAS.FirstOrDefault(x => x.Name == "Clients");
                systemClaimsList.Add(new ApplicationSystemClaim()
                {
                    ClaimType = ClientsClaimsCD.Clients_Page_ClaimType,
                    ClaimValue = ClientsClaimsCD.Clients_Page_ClaimValue,
                    Description = "Acces to view the Clients list",
                    SystemSubAreaId = clientsSubAreaId.SystemSubAreaId
                });
                //PROJECT MANAGEMENT - PROJECTS
                var projectsSubAreaId = _db.SYSTEM_SUB_AREAS.FirstOrDefault(x => x.Name == "Projects");
                systemClaimsList.Add(new ApplicationSystemClaim()
                {
                    ClaimType = ProjectsClaimsCD.Projects_Page_ClaimType,
                    ClaimValue = ProjectsClaimsCD.Projects_Page_ClaimValue,
                    Description = "Acces to view the Projects list",
                    SystemSubAreaId = projectsSubAreaId.SystemSubAreaId
                });

                foreach (var claim in systemClaimsList)
                {
                    if (_db.APPLICATION_SYSTEM_CLAIMS.FirstOrDefault(x => x.ClaimType == claim.ClaimType && x.ClaimValue == claim.ClaimValue) == null)
                    {
                        ApplicationSystemClaim asc = new()
                        {
                            ClaimType = claim.ClaimType,
                            ClaimValue = claim.ClaimValue,
                            Description = claim.Description,
                            SystemSubAreaId = claim.SystemSubAreaId
                        };
                        _db.APPLICATION_SYSTEM_CLAIMS.Add(asc);
                    }
                }
                _db.SaveChanges();
            }
            return;
        }
    }
}
