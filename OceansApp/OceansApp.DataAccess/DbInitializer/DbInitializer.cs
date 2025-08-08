using OceansApp.Models.Models;
using OceansApp.Utility;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using OceansApp.DataAccess.Data;
using OceansApp.Utility.ConstantData.Claims.AdminCenter;
using OceansApp.Utility.ConstantData.Claims.Finances;
using OceansApp.Utility.ConstantData.Claims.General;
using OceansApp.Utility.ConstantData.Claims.TrackingTool;
using OceansApp.Utility.ConstantData.Claims.AccountManagement;
using OceansApp.Utility.ConstantData.Claims.Recruiting;
using OceansApp.Models.ViewModels.ConsultantPositions;

namespace OceansApp.DataAccess.DbInitializer
{
    public class DbInitializer : IDbInitializer
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public IConfiguration _config { get; }

        public DbInitializer(
            ApplicationDbContext db,
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration config)
        {
            _db = db;
            _userManager = userManager;
            _roleManager = roleManager;
            _config = config;
        }

        public async Task InitializeAsync()
        {
            try
            {
                if ((await _db.Database.GetPendingMigrationsAsync()).Any())
                {
                    await _db.Database.MigrateAsync();
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error during executing pending migrations:");
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                Console.ResetColor();
                throw;
            }

            await using var transaction = await _db.Database.BeginTransactionAsync();
            // Speed-up massive seeding
            var originalAutoDetect = _db.ChangeTracker.AutoDetectChangesEnabled;
            _db.ChangeTracker.AutoDetectChangesEnabled = false;

            try
            {
                // ----------------- USER CATEGORIES --------------------------------

                var userCategoriesList = new List<ApplicationUserCategory>
                {
                    new() { Name = "Administrative" },
                    new() { Name = "Consultant" },
                    new() { Name = "External User" }
                };
                var existingUserCategories = (await _db.UserCategories
                   .AsNoTracking()
                   .Select(x => x.Name)
                   .ToListAsync())
                   .ToHashSet(StringComparer.OrdinalIgnoreCase);


                var userCategoriesToInsert = userCategoriesList
                    .Where(x => !existingUserCategories.Contains(x.Name))
                    .ToList();

                if (userCategoriesToInsert.Count > 0)
                    await _db.UserCategories.AddRangeAsync(userCategoriesToInsert);

                await _db.SaveChangesAsync();

                // ----------------- COMPANIES --------------------------------

                var companiesList = new List<Company>
                {
                    new() { CompanyId = "OCE", Name = "Oceans Consulting Firm, S.A" },
                    new() { CompanyId = "LLC", Name = "OCE LLC" }
                };

                var existingCompanies = await _db.COMPANIES
                    .AsNoTracking()
                    .Select(x => new { x.CompanyId, x.Name })
                    .ToListAsync();
                var existingCompanyKeys = existingCompanies
                    .Select(x => (x.CompanyId, x.Name))
                    .ToHashSet();

                var companiesToInsert = companiesList
                    .Where(c => !existingCompanyKeys.Contains((c.CompanyId, c.Name)))
                    .Select(c => new Company
                    {
                        CompanyId = c.CompanyId,
                        Name = c.Name
                    }).ToList();

                if (companiesToInsert.Count > 0)
                    await _db.COMPANIES.AddRangeAsync(companiesToInsert);

                await _db.SaveChangesAsync();

                // ----------------- COUNTRIES --------------------------------

                List<Country> countriesList = new List<Country>
                {
                    new Country { IdCountry = "ARN", Name = "Argentina" },
                    new Country { IdCountry = "BLC", Name = "Belice"},
                    new Country { IdCountry = "BOL", Name = "Bolivia" },
                    new Country { IdCountry = "BRA", Name = "Brasil" },
                    new Country { IdCountry = "CAN", Name = "Canadá"},
                    new Country { IdCountry = "CHL", Name = "Chile" },
                    new Country { IdCountry = "CHN", Name = "China" },
                    new Country { IdCountry = "CNO", Name = "República Popular Democrática de Corea"},
                    new Country { IdCountry = "COL", Name = "Colombia"},
                    new Country { IdCountry = "CRI", Name = "Costa Rica" },
                    new Country { IdCountry = "CUB", Name = "Cuba" },
                    new Country { IdCountry = "DIN", Name = "Dinamarca" },
                    new Country { IdCountry = "ECU", Name = "Ecuador" },
                    new Country { IdCountry = "ESP", Name = "España"},
                    new Country { IdCountry = "FIN", Name = "Finlandia"},
                    new Country { IdCountry = "FRA", Name = "Francia"},
                    new Country { IdCountry = "GUA", Name = "Guatemala"},
                    new Country { IdCountry = "HOL", Name = "Países Bajos"},
                    new Country { IdCountry = "HON", Name = "Honduras" },
                    new Country { IdCountry = "IDI", Name = "India"},
                    new Country { IdCountry = "ISR", Name = "Israel" },
                    new Country { IdCountry = "ITA", Name = "Italia" },
                    new Country { IdCountry = "JAM", Name = "Jamaica" },
                    new Country { IdCountry = "JAP", Name = "Japón" },
                    new Country { IdCountry = "MAL", Name = "Malasia"},
                    new Country { IdCountry = "MEX", Name = "México" },
                    new Country { IdCountry = "ND", Name = "NO DEFINIDO"},
                    new Country { IdCountry = "NIC", Name = "Nicaragua" },
                    new Country { IdCountry = "NOR", Name = "Noruega" },
                    new Country { IdCountry = "NZE", Name = "Nueva Zelandia" },
                    new Country { IdCountry = "PAN", Name = "Panamá"},
                    new Country { IdCountry = "PAR", Name = "Paraguay" },
                    new Country { IdCountry = "PER", Name = "Perú" },
                    new Country { IdCountry = "POL", Name = "Polonia"},
                    new Country { IdCountry = "POR", Name = "Portugal" },
                    new Country { IdCountry = "PRI", Name = "Puerto Rico" },
                    new Country { IdCountry = "RDO", Name = "República Dominicana" },
                    new Country { IdCountry = "RFA", Name = "Alemania" },
                    new Country { IdCountry = "RUS", Name = "Federación de Rusia" },
                    new Country { IdCountry = "SAF", Name = "Sudáfrica" },
                    new Country { IdCountry = "SAL", Name = "El Salvador" },
                    new Country { IdCountry = "SIN", Name = "Singapur"},
                    new Country { IdCountry = "SUE", Name = "Suecia" },
                    new Country { IdCountry = "SUI", Name = "Suiza" },
                    new Country { IdCountry = "SUR", Name = "Suriname"},
                    new Country { IdCountry = "TRI", Name = "Trinidad y Tobago" },
                    new Country { IdCountry = "TUR", Name = "Turquía"},
                    new Country { IdCountry = "TWN", Name = "República China Taiwan" },
                    new Country { IdCountry = "URU", Name = "Uruguay"},
                    new Country { IdCountry = "USA", Name = "Estados Unidos" },
                    new Country { IdCountry = "VEN", Name = "Venezuela"},
                    new Country { IdCountry = "YUG", Name = "Yugoslavia" },
                };

                var existingCountries = (await _db.COUNTRY
                     .AsNoTracking()
                     .Select(x => x.IdCountry)
                     .ToListAsync())
                     .ToHashSet(StringComparer.OrdinalIgnoreCase);

                var countriesToInsert = countriesList
                    .Where(c => !existingCountries.Contains(c.IdCountry))
                    .Select(c => new Country
                    {
                        IdCountry = c.IdCountry,
                        Name = c.Name,
                        CreateDate = DateTime.UtcNow
                    })
                    .ToList();

                if (countriesToInsert.Count > 0)
                    await _db.COUNTRY.AddRangeAsync(countriesToInsert);

                await _db.SaveChangesAsync();

                // ----------------- CONSULTANT BENEFITS --------------------------------

                List<ConsultantBenefit> consultantBenefitList = new List<ConsultantBenefit>
                {
                    new ConsultantBenefit { Name = "Balance Program", Amount = 750, BenefitPeriod = "Annual" },
                    new ConsultantBenefit { Name = "Bonusly", Amount = 5000, BenefitPeriod = "Undefined" },
                    new ConsultantBenefit { Name = "Oceans Challenge", Amount = 250, BenefitPeriod = "Annual" }
                };

                var existingBenefits = (await _db.CONSULTANT_BENEFITS
                    .AsNoTracking()
                    .Select(x => x.Name)
                    .ToListAsync())
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                var benefitsToInsert = consultantBenefitList
                    .Where(b => !existingBenefits.Contains(b.Name))
                    .Select(b => new ConsultantBenefit
                    {
                        Name = b.Name,
                        Amount = b.Amount,
                        BenefitPeriod = b.BenefitPeriod
                    }).ToList();

                if (benefitsToInsert.Count > 0)
                    await _db.CONSULTANT_BENEFITS.AddRangeAsync(benefitsToInsert);

                await _db.SaveChangesAsync();

                // ----------------- CONSULTANT BENEFITS CATEGORIES --------------------------------

                // Look up inserted benefits by name to get IDs safely
                var balanceProgramBenefit = await _db.CONSULTANT_BENEFITS.AsNoTracking().FirstOrDefaultAsync(x => x.Name == "Balance Program")
                    ?? throw new InvalidOperationException("Benefit 'Balance Program' not found after insertion.");
                var bonuslyBenefit = await _db.CONSULTANT_BENEFITS.AsNoTracking().FirstOrDefaultAsync(x => x.Name == "Bonusly")
                    ?? throw new InvalidOperationException("Benefit 'Bonusly' not found after insertion.");
                var oceansChallengeBenefit = await _db.CONSULTANT_BENEFITS.AsNoTracking().FirstOrDefaultAsync(x => x.Name == "Oceans Challenge")
                    ?? throw new InvalidOperationException("Benefit 'Oceans Challenge' not found after insertion.");

                var consultantBenefitCategoriesList = new List<ConsultantBenefitCategory>
                {
                    new() { Name = "Expert Boost ($250) (2500 Bonus.ly XP)", BenefitId = balanceProgramBenefit.BenefitId },
                    new() { Name = "Wellness Coverage ($750)", BenefitId = balanceProgramBenefit.BenefitId },

                    new() { Name = "Curiosity Stream 1 year ($25)", BenefitId = bonuslyBenefit.BenefitId },
                    new() { Name = "A new gaming console ($500)", BenefitId = bonuslyBenefit.BenefitId },
                    new() { Name = "Adventure tickets ($100)", BenefitId = bonuslyBenefit.BenefitId },
                    new() { Name = "Buy a book! ($25)", BenefitId = bonuslyBenefit.BenefitId },
                    new() { Name = "Ergonomics ($150)", BenefitId = bonuslyBenefit.BenefitId },
                    new() { Name = "Gamers ($200)", BenefitId = bonuslyBenefit.BenefitId },
                    new() { Name = "Hotel or plane tickets ($240/$480/$750)", BenefitId = bonuslyBenefit.BenefitId },
                    new() { Name = "Just Cash Out ($50/$100/$200)", BenefitId = bonuslyBenefit.BenefitId },
                    new() { Name = "Lodgings ($100) ", BenefitId = bonuslyBenefit.BenefitId },
                    new() { Name = "Movie Night ($30)", BenefitId = bonuslyBenefit.BenefitId },
                    new() { Name = "Music Lovers! ($60)", BenefitId = bonuslyBenefit.BenefitId },
                    new() { Name = "N. Fitness Freaks ($120)", BenefitId = bonuslyBenefit.BenefitId },
                    new() { Name = "Nintendo Switch ONLINE 1 year ($40)", BenefitId = bonuslyBenefit.BenefitId },
                    new() { Name = "Out for dinner ($80)", BenefitId = bonuslyBenefit.BenefitId },
                    new() { Name = "Personal Care ($35/$70/$140)", BenefitId = bonuslyBenefit.BenefitId },
                    new() { Name = "PlayStation Plus ($60)", BenefitId = bonuslyBenefit.BenefitId },
                    new() { Name = "Streaming Subscriptions ($20)", BenefitId = bonuslyBenefit.BenefitId },
                    new() { Name = "Tech gadgets I ($30)", BenefitId = bonuslyBenefit.BenefitId },
                    new() { Name = "Tech gadgets II ($140)", BenefitId = bonuslyBenefit.BenefitId },
                    new() { Name = "Tech gadgets III ($300)", BenefitId = bonuslyBenefit.BenefitId },
                    new() { Name = "UberEats voucher ($25)", BenefitId = bonuslyBenefit.BenefitId },

                    new() { Name = "Courses (in person/online)", BenefitId = oceansChallengeBenefit.BenefitId },
                    new() { Name = "Licenses for learning tools and work support", BenefitId = oceansChallengeBenefit.BenefitId },
                    new() { Name = "Universities Enrollment", BenefitId = oceansChallengeBenefit.BenefitId },
                    new() { Name = "Certificates", BenefitId = oceansChallengeBenefit.BenefitId }
                };

                var existingBenefitCategories = (await _db.CONSULTANT_BENEFIT_CATEGORIES
                    .AsNoTracking()
                    .Select(x => x.Name)
                    .ToListAsync())
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                var benefitCategoriesToInsert = consultantBenefitCategoriesList
                    .Where(c => !existingBenefitCategories.Contains(c.Name))
                    .Select(c => new ConsultantBenefitCategory
                    {
                        Name = c.Name,
                        BenefitId = c.BenefitId
                    }).ToList();

                if (benefitCategoriesToInsert.Count > 0)
                    await _db.CONSULTANT_BENEFIT_CATEGORIES.AddRangeAsync(benefitCategoriesToInsert);

                await _db.SaveChangesAsync();

                // ----------------- DEFAULT COST CENTERS --------------------------------

                var costCentersList = new List<CostCenter>
                {
                    new() { CostCenterCode = "10-02-04", Description = "Area People and Culture", Detail = "Gastos especificos que abarcan el Área de People and Culture, como: Beneficios, regalías y presentes a personal, actividades sociales, etc.", AcceptData = "S", CompanyId = "OCE", CreateDate = DateTime.UtcNow },
                    new() { CostCenterCode = "10-02-04", Description = "Area People and Culture", Detail = null, AcceptData = "S", CompanyId = "LLC", CreateDate = DateTime.UtcNow },
                    new() { CostCenterCode = "50-01-00", Description = "Area Finanzas", Detail = "Gastos especificos que abarcan el Área de Finanzas, como: Herramientas contables, Servicios contables, Licencias en esta área, etc.", AcceptData = "S", CompanyId = "OCE", CreateDate = DateTime.UtcNow },
                    new() { CostCenterCode = "50-01-00", Description = "Area Finanzas", Detail = null, AcceptData = "S", CompanyId = "LLC", CreateDate = DateTime.UtcNow },
                    new() { CostCenterCode = "30-01-01", Description = "Area Reclutamiento", Detail = "Gastos especificos que abarcan el Área de Recursos Humanos, como: Pagos a reclutadoras, Pagos a entrevistadores, Licencias en esta área, etc.", AcceptData = "S", CompanyId = "OCE", CreateDate = DateTime.UtcNow },
                    new() { CostCenterCode = "30-01-01", Description = "Area Reclutamiento", Detail = null, AcceptData = "S", CompanyId = "LLC", CreateDate = DateTime.UtcNow },
                    new() { CostCenterCode = "10-02-02", Description = "Area ejecutivos de cuentas", Detail = "Gastos especificos que abarcan el Área de Ejecutivos de Cuentas, como: Pagos a ejecutivos de cuentas, Licencias en esta área, etc.", AcceptData = "S", CompanyId = "OCE", CreateDate = DateTime.UtcNow },
                    new() { CostCenterCode = "10-02-02", Description = "Area ejecutivos de cuentas", Detail = null, AcceptData = "S", CompanyId = "LLC", CreateDate = DateTime.UtcNow },
                    new() { CostCenterCode = "40-01-01", Description = "Operaciones Recursos", Detail = "Gastos en los que se incurre para que los Desarrolladores y QAs puedan trabajar, como: tracking tools, envíos de computadoras, etc.", AcceptData = "S", CompanyId = "OCE", CreateDate = DateTime.UtcNow },
                    new() { CostCenterCode = "40-01-01", Description = "Operaciones Recursos", Detail = null, AcceptData = "S", CompanyId = "LLC", CreateDate = DateTime.UtcNow }
                };

                var existingCostCenters = await _db.COST_CENTER
                    .AsNoTracking()
                    .Select(x => new { x.CostCenterCode, x.CompanyId })
                    .ToListAsync();
                var existingCostCenterKeys = existingCostCenters
                    .Select(x => (x.CostCenterCode, x.CompanyId))
                    .ToHashSet();

                var costCentersToInsert = costCentersList
                    .Where(c => !existingCostCenterKeys.Contains((c.CostCenterCode, c.CompanyId)))
                    .ToList();

                if (costCentersToInsert.Count > 0)
                    await _db.COST_CENTER.AddRangeAsync(costCentersToInsert);

                await _db.SaveChangesAsync();

                // ----------------- DEFAULT ACCOUNTING ACCOUNTS --------------------------------

                var accountingAccountsList = new List<AccountingAccount>
                {
                    new() { Description = "Reserva para beneficios (Balance Program)", AccountingAccountType = "B", DetailedType = "T", Balance = "A", AcceptData = "S", UseCostCenter = "S", UseThird = "S", AccountingAccountCode = "3-02-01-000-000", CompanyId = "OCE", DescriptionIFRS = "Reserva para beneficios (Balance Program)", DateLastUpdate = DateTime.UtcNow, DateHour = DateTime.UtcNow },
                    new() { Description = "Reserva para beneficios (Bonusly)", AccountingAccountType = "B", DetailedType = "T", Balance = "A", AcceptData = "S", UseCostCenter = "S", UseThird = "S", AccountingAccountCode = "3-02-02-000-000", CompanyId = "OCE", DescriptionIFRS = "Reserva para beneficios (Bonusly)", DateLastUpdate = DateTime.UtcNow, DateHour = DateTime.UtcNow },
                    new() { Description = "Cursos / Capacitaciones (Oceans Challenge)", AccountingAccountType = "E", DetailedType = "G", Balance = "D", AcceptData = "S", UseCostCenter = "S", UseThird = "S", AccountingAccountCode = "6-01-03-005-000", CompanyId = "OCE", DescriptionIFRS = "Cursos / Capacitaciones (Oceans Challenge)", DateLastUpdate = DateTime.UtcNow, DateHour = DateTime.UtcNow },
                    new() { Description = "Administrative expenses for OCE", AccountingAccountType = "E", DetailedType = "G", Balance = "D", AcceptData = "S", UseCostCenter = "S", UseThird = "N", AccountingAccountCode = "6-01-04-013-0000", CompanyId = "LLC", DescriptionIFRS = "Gastos administrativos para OCE", DateLastUpdate = DateTime.UtcNow, DateHour = DateTime.UtcNow },
                    new() { Description = "Pago a recursos Administrativos", AccountingAccountType = "E", DetailedType = "G", Balance = "D", AcceptData = "S", UseCostCenter = "S", UseThird = "S", AccountingAccountCode = "6-01-01-001-000", CompanyId = "OCE", DescriptionIFRS = "Horas y salarios a personal administrativo", DateLastUpdate = DateTime.UtcNow, DateHour = DateTime.UtcNow },
                    new() { Description = "Payment to administrative consultants", AccountingAccountType = "E", DetailedType = "G", Balance = "D", AcceptData = "S", UseCostCenter = "S", UseThird = "S", AccountingAccountCode = "6-01-01-001-0000", CompanyId = "LLC", DescriptionIFRS = "Pago a recursos Administrativos", DateLastUpdate = DateTime.UtcNow, DateHour = DateTime.UtcNow },
                    new() { Description = "Días Feriados Administrativos", AccountingAccountType = "E", DetailedType = "G", Balance = "D", AcceptData = "S", UseCostCenter = "S", UseThird = "S", AccountingAccountCode = "6-01-01-002-000", CompanyId = "OCE", DescriptionIFRS = "Feriados Administrativos", DateLastUpdate = DateTime.UtcNow, DateHour = DateTime.UtcNow },
                    new() { Description = "Administrative consultant holidays", AccountingAccountType = "E", DetailedType = "G", Balance = "D", AcceptData = "S", UseCostCenter = "S", UseThird = "S", AccountingAccountCode = "6-01-01-002-0000", CompanyId = "LLC", DescriptionIFRS = "Días Feriados Administrativos", DateLastUpdate = DateTime.UtcNow, DateHour = DateTime.UtcNow },
                    new() { Description = "Entrevistas con Prospectos", AccountingAccountType = "E", DetailedType = "G", Balance = "D", AcceptData = "S", UseCostCenter = "S", UseThird = "S", AccountingAccountCode = "6-01-01-005-000", CompanyId = "OCE", DescriptionIFRS = "Horas adicionales pagadas a personas que participan en entrevistas", DateLastUpdate = DateTime.UtcNow, DateHour = DateTime.UtcNow },
                    new() { Description = "Pagos a ejecutivos de cuenta", AccountingAccountType = "E", DetailedType = "G", Balance = "D", AcceptData = "S", UseCostCenter = "S", UseThird = "S", AccountingAccountCode = "6-01-01-007-000", CompanyId = "OCE", DescriptionIFRS = "Pagos a ejecutivos de cuenta", DateLastUpdate = DateTime.UtcNow, DateHour = DateTime.UtcNow },
                    new() { Description = "Horas de recursos", AccountingAccountType = "E", DetailedType = "G", Balance = "D", AcceptData = "S", UseCostCenter = "S", UseThird = "S", AccountingAccountCode = "5-01-01-000-000", CompanyId = "OCE", DescriptionIFRS = "Horas de recursos", DateLastUpdate = DateTime.UtcNow, DateHour = DateTime.UtcNow },
                    new() { Description = "Consultant hours", AccountingAccountType = "E", DetailedType = "G", Balance = "D", AcceptData = "S", UseCostCenter = "S", UseThird = "S", AccountingAccountCode = "5-01-01-000-0000", CompanyId = "LLC", DescriptionIFRS = "Horas de recursos", DateLastUpdate = DateTime.UtcNow, DateHour = DateTime.UtcNow },
                    new() { Description = "Días Feriados de Recursos", AccountingAccountType = "E", DetailedType = "G", Balance = "D", AcceptData = "S", UseCostCenter = "S", UseThird = "S", AccountingAccountCode = "5-01-06-000-000", CompanyId = "OCE", DescriptionIFRS = "Días Feriados de Recursos", DateLastUpdate = DateTime.UtcNow, DateHour = DateTime.UtcNow },
                    new() { Description = "Consultant holidays", AccountingAccountType = "E", DetailedType = "G", Balance = "D", AcceptData = "S", UseCostCenter = "S", UseThird = "S", AccountingAccountCode = "5-01-03-000-0000", CompanyId = "LLC", DescriptionIFRS = "Días Feriados de Recursos", DateLastUpdate = DateTime.UtcNow, DateHour = DateTime.UtcNow },
                    new() { Description = "Tarifa de guardia (On Call)", AccountingAccountType = "E", DetailedType = "G", Balance = "D", AcceptData = "S", UseCostCenter = "S", UseThird = "S", AccountingAccountCode = "5-01-16-000-000", CompanyId = "OCE", DescriptionIFRS = "Tarifa de guardia (On Call)", DateLastUpdate = DateTime.UtcNow, DateHour = DateTime.UtcNow },
                    new() { Description = "On Call flate rate", AccountingAccountType = "E", DetailedType = "G", Balance = "D", AcceptData = "S", UseCostCenter = "S", UseThird = "N", AccountingAccountCode = "5-01-08-000-0000", CompanyId = "LLC", DescriptionIFRS = "Tarifa de guardia (On call)", DateLastUpdate = DateTime.UtcNow, DateHour = DateTime.UtcNow },
                    new() { Description = "Horas en On Call trabajadas", AccountingAccountType = "E", DetailedType = "G", Balance = "D", AcceptData = "S", UseCostCenter = "S", UseThird = "N", AccountingAccountCode = "5-01-22-000-000", CompanyId = "OCE", DescriptionIFRS = "Horas en On Call trabajadas", DateLastUpdate = DateTime.UtcNow, DateHour = DateTime.UtcNow },
                    new() { Description = "On Call hours worked", AccountingAccountType = "E", DetailedType = "G", Balance = "D", AcceptData = "S", UseCostCenter = "S", UseThird = "N", AccountingAccountCode = "5-01-09-000-0000", CompanyId = "LLC", DescriptionIFRS = "Horas trabajadas en On Call", DateLastUpdate = DateTime.UtcNow, DateHour = DateTime.UtcNow }
                };

                var existingAccounts = await _db.ACCOUNTING_ACCOUNT
                    .AsNoTracking()
                    .Select(x => new { x.AccountingAccountCode, x.CompanyId })
                    .ToListAsync();
                var existingAccountKeys = existingAccounts
                    .Select(x => (x.AccountingAccountCode, x.CompanyId))
                    .ToHashSet();

                var accountsToInsert = accountingAccountsList
                    .Where(a => !existingAccountKeys.Contains((a.AccountingAccountCode, a.CompanyId)))
                    .ToList();

                if (accountsToInsert.Count > 0)
                    await _db.ACCOUNTING_ACCOUNT.AddRangeAsync(accountsToInsert);

                await _db.SaveChangesAsync();

                // ----------------- DEFAULT COSTS CENTERS AND ACCOUNTING ACCOUNTS RELATIONSHIP --------------------------------

                // Build fast lookup dictionaries (code+company -> Id)
                var ccByCodeCompany = await _db.COST_CENTER
                    .AsNoTracking()
                    .ToDictionaryAsync(x => (x.CostCenterCode, x.CompanyId), x => x.CostCenterId);

                var accByCodeCompany = await _db.ACCOUNTING_ACCOUNT
                    .AsNoTracking()
                    .ToDictionaryAsync(x => (x.AccountingAccountCode, x.CompanyId), x => x.AccountingAccountId);

                // Helper to get ID or throw with a clear message
                int CC(string code, string company) =>
                    ccByCodeCompany.TryGetValue((code, company), out var id)
                        ? id
                        : throw new InvalidOperationException($"Cost center '{code}' (Company '{company}') not found.");

                int AA(string code, string company) =>
                    accByCodeCompany.TryGetValue((code, company), out var id)
                        ? id
                        : throw new InvalidOperationException($"Accounting account '{code}' (Company '{company}') not found.");

                var costsCentersAccountingAccountsList = new List<CostCenterAccountingAccount>
                {
                    new() { CostCenterId = CC("10-02-04","OCE"), AccountingAccountId = AA("3-02-01-000-000","OCE"), Status = "A", CompanyId = "OCE" },
                    new() { CostCenterId = CC("10-02-04","LLC"), AccountingAccountId = AA("6-01-04-013-0000","LLC"), Status = "A", CompanyId = "LLC" },
                    new() { CostCenterId = CC("10-02-04","OCE"), AccountingAccountId = AA("6-01-03-005-000","OCE"), Status = "A", CompanyId = "OCE" },
                    new() { CostCenterId = CC("10-02-04","OCE"), AccountingAccountId = AA("3-02-02-000-000","OCE"), Status = "A", CompanyId = "OCE" },
                    new() { CostCenterId = CC("50-01-00","OCE"), AccountingAccountId = AA("6-01-01-001-000","OCE"), Status = "A", CompanyId = "OCE" },
                    new() { CostCenterId = CC("50-01-00","LLC"), AccountingAccountId = AA("6-01-04-013-0000","LLC"), Status = "A", CompanyId = "LLC" },
                    new() { CostCenterId = CC("50-01-00","OCE"), AccountingAccountId = AA("6-01-01-002-000","OCE"), Status = "A", CompanyId = "OCE" },
                    new() { CostCenterId = CC("30-01-01","OCE"), AccountingAccountId = AA("6-01-01-001-000","OCE"), Status = "A", CompanyId = "OCE" },
                    new() { CostCenterId = CC("30-01-01","LLC"), AccountingAccountId = AA("6-01-04-013-0000","LLC"), Status = "A", CompanyId = "LLC" },
                    new() { CostCenterId = CC("10-02-04","OCE"), AccountingAccountId = AA("6-01-01-002-000","OCE"), Status = "A", CompanyId = "OCE" },
                    new() { CostCenterId = CC("30-01-01","OCE"), AccountingAccountId = AA("6-01-01-005-000","OCE"), Status = "A", CompanyId = "OCE" },
                    new() { CostCenterId = CC("30-01-01","OCE"), AccountingAccountId = AA("6-01-01-002-000","OCE"), Status = "A", CompanyId = "OCE" },
                    new() { CostCenterId = CC("10-02-02","OCE"), AccountingAccountId = AA("6-01-01-007-000","OCE"), Status = "A", CompanyId = "OCE" },
                    new() { CostCenterId = CC("10-02-02","OCE"), AccountingAccountId = AA("6-01-01-002-000","OCE"), Status = "A", CompanyId = "OCE" },
                    new() { CostCenterId = CC("10-02-02","LLC"), AccountingAccountId = AA("6-01-04-013-0000","LLC"), Status = "A", CompanyId = "LLC" },
                    new() { CostCenterId = CC("40-01-01","OCE"), AccountingAccountId = AA("5-01-01-000-000","OCE"), Status = "A", CompanyId = "OCE" },
                    new() { CostCenterId = CC("40-01-01","LLC"), AccountingAccountId = AA("5-01-01-000-0000","LLC"), Status = "A", CompanyId = "LLC" },
                    new() { CostCenterId = CC("40-01-01","OCE"), AccountingAccountId = AA("5-01-06-000-000","OCE"), Status = "A", CompanyId = "OCE" },
                    new() { CostCenterId = CC("40-01-01","LLC"), AccountingAccountId = AA("5-01-03-000-0000","LLC"), Status = "A", CompanyId = "LLC" },
                    new() { CostCenterId = CC("40-01-01","OCE"), AccountingAccountId = AA("5-01-16-000-000","OCE"), Status = "A", CompanyId = "OCE" },
                    new() { CostCenterId = CC("40-01-01","LLC"), AccountingAccountId = AA("5-01-08-000-0000","LLC"), Status = "A", CompanyId = "LLC" },
                    new() { CostCenterId = CC("40-01-01","OCE"), AccountingAccountId = AA("5-01-22-000-000","OCE"), Status = "A", CompanyId = "OCE" },
                    new() { CostCenterId = CC("40-01-01","LLC"), AccountingAccountId = AA("5-01-09-000-0000","LLC"), Status = "A", CompanyId = "LLC" }
                };

                // Prevent duplicates
                var existingCCAARels = await _db.COSTS_CENTERS_ACCOUNTING_ACCOUNTS
                    .AsNoTracking()
                    .Select(x => new { x.CostCenterId, x.AccountingAccountId, x.CompanyId })
                    .ToListAsync();
                var existingCCAARelKeys = existingCCAARels
                    .Select(x => (x.CostCenterId, x.AccountingAccountId, x.CompanyId))
                    .ToHashSet();

                var ccaaToInsert = costsCentersAccountingAccountsList
                    .Where(r => !existingCCAARelKeys.Contains((r.CostCenterId, r.AccountingAccountId, r.CompanyId)))
                    .Select(r => new CostCenterAccountingAccount
                    {
                        CostCenterId = r.CostCenterId,
                        AccountingAccountId = r.AccountingAccountId,
                        Status = r.Status,
                        CreateDate = DateTime.UtcNow,
                        CompanyId = r.CompanyId
                    }).ToList();

                if (ccaaToInsert.Count > 0)
                    await _db.COSTS_CENTERS_ACCOUNTING_ACCOUNTS.AddRangeAsync(ccaaToInsert);

                await _db.SaveChangesAsync();

                // ----------------- CONSULTANT BENEFITS COMPANIES --------------------------------

                // Lookups needed
                var peopleAndCultureCostCenterOCE = CC("10-02-04", "OCE");
                var peopleAndCultureCostCenterLLC = CC("10-02-04", "LLC");
                var accountingAccountReservaBalanceProgramOCE = AA("3-02-01-000-000", "OCE");
                var accountingAccountAdminExpensesLLC = AA("6-01-04-013-0000", "LLC");

                var consultantBenefitCompaniesList = new List<ConsultantBenefitCompany>
                {
                    // OCE
                    new() { CompanyId = "OCE", CostCenterId = peopleAndCultureCostCenterOCE, AccountingAccountId = accountingAccountReservaBalanceProgramOCE, BenefitId = balanceProgramBenefit.BenefitId },
                    new() { CompanyId = "OCE", CostCenterId = peopleAndCultureCostCenterOCE, AccountingAccountId = AA("3-02-02-000-000","OCE"), BenefitId = bonuslyBenefit.BenefitId },
                    new() { CompanyId = "OCE", CostCenterId = peopleAndCultureCostCenterOCE, AccountingAccountId = AA("6-01-03-005-000","OCE"), BenefitId = oceansChallengeBenefit.BenefitId },

                    // LLC
                    new() { CompanyId = "LLC", CostCenterId = peopleAndCultureCostCenterLLC, AccountingAccountId = accountingAccountAdminExpensesLLC, BenefitId = balanceProgramBenefit.BenefitId },
                    new() { CompanyId = "LLC", CostCenterId = peopleAndCultureCostCenterLLC, AccountingAccountId = accountingAccountAdminExpensesLLC, BenefitId = bonuslyBenefit.BenefitId },
                    new() { CompanyId = "LLC", CostCenterId = peopleAndCultureCostCenterLLC, AccountingAccountId = accountingAccountAdminExpensesLLC, BenefitId = oceansChallengeBenefit.BenefitId }
                };

                var existingBenefitCompanies = await _db.CONSULTANT_BENEFIT_COMPANIES
                    .AsNoTracking()
                    .Select(x => new { x.CompanyId, x.CostCenterId, x.AccountingAccountId, x.BenefitId })
                    .ToListAsync();
                var existingBenefitCompanyKeys = existingBenefitCompanies
                    .Select(x => (x.CompanyId, x.CostCenterId, x.AccountingAccountId, x.BenefitId))
                    .ToHashSet();

                var benefitCompaniesToInsert = consultantBenefitCompaniesList
                    .Where(bc => !existingBenefitCompanyKeys.Contains((bc.CompanyId, bc.CostCenterId, bc.AccountingAccountId, bc.BenefitId)))
                    .ToList();

                if (benefitCompaniesToInsert.Count > 0)
                    await _db.CONSULTANT_BENEFIT_COMPANIES.AddRangeAsync(benefitCompaniesToInsert);

                await _db.SaveChangesAsync();

                // ----------------- REPORTING MY TIME MOVEMENT TYPES --------------------------------

                var movTypesList = new List<ReportingMyTimeMovementType>
                {
                    new() { Name = "Normal Hours",         IsPayable = true  },
                    new() { Name = "On Call Flate Rate",   IsPayable = true  },
                    new() { Name = "On Call Time Worked",  IsPayable = true  },
                    new() { Name = "Balance Program",      IsPayable = true  },
                    new() { Name = "Oceans Challenge",     IsPayable = true  },
                    new() { Name = "Bonusly Rewards",      IsPayable = true  },
                    new() { Name = "Interviews",           IsPayable = true  },
                    new() { Name = "Time Off (Non-payable)", IsPayable = false },
                    new() { Name = "Holidays",             IsPayable = true  },
                    new() { Name = "Overtime Hours",       IsPayable = false }
                };

                var existingMovementTypes = (await _db.REPORTING_MY_TIME_MOVEMENT_TYPES
                    .AsNoTracking()
                    .Select(x => x.Name)
                    .ToListAsync())
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                var movTypesToInsert = movTypesList
                    .Where(m => !existingMovementTypes.Contains(m.Name))
                    .ToList();

                if (movTypesToInsert.Count > 0)
                    await _db.REPORTING_MY_TIME_MOVEMENT_TYPES.AddRangeAsync(movTypesToInsert);

                await _db.SaveChangesAsync();

                // ----------------- CONSULTANT POSITIONS BY DEFAULT --------------------------------

                var hasPositions = await _db.CONSULTANT_POSITIONS.AsNoTracking().AnyAsync();
                if (!hasPositions)
                {
                    // Source configuration (same as original, grouped)
                    var consultantPositionsList = new List<CreatePositionsWithAccountingConfigVM>();

                    // People and Culture Coordinator
                    consultantPositionsList.Add(new CreatePositionsWithAccountingConfigVM
                    {
                        Name = "People and Culture Coordinator",
                        IsAdministrative = true,
                        AccountingConfig = new List<CreateAccountingConfigVM>
                        {
                            // LLC
                            new() { MovementTypeName = "Balance Program",   CompanyId = "LLC", CostCenterCode = "10-02-04", AccountingAccountCode = "6-01-04-013-0000" },
                            new() { MovementTypeName = "Bonusly Rewards",   CompanyId = "LLC", CostCenterCode = "10-02-04", AccountingAccountCode = "6-01-04-013-0000" },
                            new() { MovementTypeName = "Holidays",          CompanyId = "LLC", CostCenterCode = "10-02-04", AccountingAccountCode = "6-01-04-013-0000" },
                            new() { MovementTypeName = "Interviews",        CompanyId = "LLC", CostCenterCode = "30-01-01", AccountingAccountCode = "6-01-04-013-0000" },
                            new() { MovementTypeName = "Normal Hours",      CompanyId = "LLC", CostCenterCode = "10-02-04", AccountingAccountCode = "6-01-04-013-0000" },
                            new() { MovementTypeName = "Oceans Challenge",  CompanyId = "LLC", CostCenterCode = "10-02-04", AccountingAccountCode = "6-01-04-013-0000" },
                            new() { MovementTypeName = "On Call Flate Rate",CompanyId = "LLC", CostCenterCode = "10-02-04", AccountingAccountCode = "6-01-04-013-0000" },
                            new() { MovementTypeName = "On Call Time Worked",CompanyId = "LLC", CostCenterCode = "10-02-04", AccountingAccountCode = "6-01-04-013-0000" },

                            // OCE
                            new() { MovementTypeName = "Balance Program",   CompanyId = "OCE", CostCenterCode = "10-02-04", AccountingAccountCode = "3-02-01-000-000" },
                            new() { MovementTypeName = "Bonusly Rewards",   CompanyId = "OCE", CostCenterCode = "10-02-04", AccountingAccountCode = "3-02-02-000-000" },
                            new() { MovementTypeName = "Holidays",          CompanyId = "OCE", CostCenterCode = "10-02-04", AccountingAccountCode = "6-01-01-002-000" },
                            new() { MovementTypeName = "Interviews",        CompanyId = "OCE", CostCenterCode = "30-01-01", AccountingAccountCode = "6-01-01-005-000" },
                            new() { MovementTypeName = "Normal Hours",      CompanyId = "OCE", CostCenterCode = "10-02-04", AccountingAccountCode = "6-01-01-001-000" },
                            new() { MovementTypeName = "Oceans Challenge",  CompanyId = "OCE", CostCenterCode = "10-02-04", AccountingAccountCode = "6-01-03-005-000" },
                            new() { MovementTypeName = "On Call Flate Rate",CompanyId = "OCE", CostCenterCode = "10-02-04", AccountingAccountCode = "6-01-01-001-000" },
                            new() { MovementTypeName = "On Call Time Worked",CompanyId = "OCE", CostCenterCode = "10-02-04", AccountingAccountCode = "6-01-01-001-000" },
                        }
                    });

                    // Success Manager
                    consultantPositionsList.Add(new CreatePositionsWithAccountingConfigVM
                    {
                        Name = "Success Manager",
                        IsAdministrative = true,
                        AccountingConfig = new List<CreateAccountingConfigVM>
                        {
                            // LLC
                            new() { MovementTypeName = "Balance Program",   CompanyId = "LLC", CostCenterCode = "10-02-04", AccountingAccountCode = "6-01-04-013-0000" },
                            new() { MovementTypeName = "Bonusly Rewards",   CompanyId = "LLC", CostCenterCode = "10-02-04", AccountingAccountCode = "6-01-04-013-0000" },
                            new() { MovementTypeName = "Holidays",          CompanyId = "LLC", CostCenterCode = "10-02-02", AccountingAccountCode = "6-01-04-013-0000" },
                            new() { MovementTypeName = "Interviews",        CompanyId = "LLC", CostCenterCode = "30-01-01", AccountingAccountCode = "6-01-04-013-0000" },
                            new() { MovementTypeName = "Normal Hours",      CompanyId = "LLC", CostCenterCode = "10-02-02", AccountingAccountCode = "6-01-04-013-0000" },
                            new() { MovementTypeName = "Oceans Challenge",  CompanyId = "LLC", CostCenterCode = "10-02-04", AccountingAccountCode = "6-01-04-013-0000" },
                            new() { MovementTypeName = "On Call Flate Rate",CompanyId = "LLC", CostCenterCode = "10-02-02", AccountingAccountCode = "6-01-04-013-0000" },
                            new() { MovementTypeName = "On Call Time Worked",CompanyId = "LLC", CostCenterCode = "10-02-02", AccountingAccountCode = "6-01-04-013-0000" },

                            // OCE
                            new() { MovementTypeName = "Balance Program",   CompanyId = "OCE", CostCenterCode = "10-02-04", AccountingAccountCode = "3-02-01-000-000" },
                            new() { MovementTypeName = "Bonusly Rewards",   CompanyId = "OCE", CostCenterCode = "10-02-04", AccountingAccountCode = "3-02-02-000-000" },
                            new() { MovementTypeName = "Holidays",          CompanyId = "OCE", CostCenterCode = "10-02-02", AccountingAccountCode = "6-01-01-002-000" },
                            new() { MovementTypeName = "Interviews",        CompanyId = "OCE", CostCenterCode = "30-01-01", AccountingAccountCode = "6-01-01-005-000" },
                            new() { MovementTypeName = "Normal Hours",      CompanyId = "OCE", CostCenterCode = "10-02-02", AccountingAccountCode = "6-01-01-007-000" },
                            new() { MovementTypeName = "Oceans Challenge",  CompanyId = "OCE", CostCenterCode = "10-02-04", AccountingAccountCode = "6-01-03-005-000" },
                            new() { MovementTypeName = "On Call Flate Rate",CompanyId = "OCE", CostCenterCode = "10-02-02", AccountingAccountCode = "6-01-01-007-000" },
                            new() { MovementTypeName = "On Call Time Worked",CompanyId = "OCE", CostCenterCode = "10-02-02", AccountingAccountCode = "6-01-01-007-000" }
                        }
                    });

                    // Finance Assistant
                    consultantPositionsList.Add(new CreatePositionsWithAccountingConfigVM
                    {
                        Name = "Finance Assistant",
                        IsAdministrative = true,
                        AccountingConfig = new List<CreateAccountingConfigVM>
                        {
                            // LLC
                            new() { MovementTypeName = "Balance Program",   CompanyId = "LLC", CostCenterCode = "10-02-04", AccountingAccountCode = "6-01-04-013-0000" },
                            new() { MovementTypeName = "Bonusly Rewards",   CompanyId = "LLC", CostCenterCode = "10-02-04", AccountingAccountCode = "6-01-04-013-0000" },
                            new() { MovementTypeName = "Holidays",          CompanyId = "LLC", CostCenterCode = "50-01-00", AccountingAccountCode = "6-01-04-013-0000" },
                            new() { MovementTypeName = "Interviews",        CompanyId = "LLC", CostCenterCode = "30-01-01", AccountingAccountCode = "6-01-04-013-0000" },
                            new() { MovementTypeName = "Normal Hours",      CompanyId = "LLC", CostCenterCode = "50-01-00", AccountingAccountCode = "6-01-04-013-0000" },
                            new() { MovementTypeName = "Oceans Challenge",  CompanyId = "LLC", CostCenterCode = "10-02-04", AccountingAccountCode = "6-01-04-013-0000" },
                            new() { MovementTypeName = "On Call Flate Rate",CompanyId = "LLC", CostCenterCode = "50-01-00", AccountingAccountCode = "6-01-04-013-0000" },
                            new() { MovementTypeName = "On Call Time Worked",CompanyId = "LLC", CostCenterCode = "50-01-00", AccountingAccountCode = "6-01-04-013-0000" },

                            // OCE
                            new() { MovementTypeName = "Balance Program",   CompanyId = "OCE", CostCenterCode = "10-02-04", AccountingAccountCode = "3-02-01-000-000" },
                            new() { MovementTypeName = "Bonusly Rewards",   CompanyId = "OCE", CostCenterCode = "10-02-04", AccountingAccountCode = "3-02-02-000-000" },
                            new() { MovementTypeName = "Holidays",          CompanyId = "OCE", CostCenterCode = "50-01-00", AccountingAccountCode = "6-01-01-002-000" },
                            new() { MovementTypeName = "Interviews",        CompanyId = "OCE", CostCenterCode = "30-01-01", AccountingAccountCode = "6-01-01-005-000" },
                            new() { MovementTypeName = "Normal Hours",      CompanyId = "OCE", CostCenterCode = "50-01-00", AccountingAccountCode = "6-01-01-001-000" },
                            new() { MovementTypeName = "Oceans Challenge",  CompanyId = "OCE", CostCenterCode = "10-02-04", AccountingAccountCode = "6-01-03-005-000" },
                            new() { MovementTypeName = "On Call Flate Rate",CompanyId = "OCE", CostCenterCode = "50-01-00", AccountingAccountCode = "6-01-01-001-000" },
                            new() { MovementTypeName = "On Call Time Worked",CompanyId = "OCE", CostCenterCode = "50-01-00", AccountingAccountCode = "6-01-01-001-000" }
                        }
                    });

                    // Full Stack Developer
                    consultantPositionsList.Add(new CreatePositionsWithAccountingConfigVM
                    {
                        Name = "Full Stack Developer",
                        IsAdministrative = false,
                        AccountingConfig = new List<CreateAccountingConfigVM>
                        {
                            // LLC
                            new() { MovementTypeName = "Balance Program",   CompanyId = "LLC", CostCenterCode = "10-02-04", AccountingAccountCode = "6-01-04-013-0000" },
                            new() { MovementTypeName = "Bonusly Rewards",   CompanyId = "LLC", CostCenterCode = "10-02-04", AccountingAccountCode = "6-01-04-013-0000" },
                            new() { MovementTypeName = "Holidays",          CompanyId = "LLC", CostCenterCode = "40-01-01", AccountingAccountCode = "5-01-03-000-0000" },
                            new() { MovementTypeName = "Interviews",        CompanyId = "LLC", CostCenterCode = "30-01-01", AccountingAccountCode = "6-01-04-013-0000" },
                            new() { MovementTypeName = "Normal Hours",      CompanyId = "LLC", CostCenterCode = "40-01-01", AccountingAccountCode = "5-01-01-000-0000" },
                            new() { MovementTypeName = "Oceans Challenge",  CompanyId = "LLC", CostCenterCode = "10-02-04", AccountingAccountCode = "6-01-04-013-0000" },
                            new() { MovementTypeName = "On Call Flate Rate",CompanyId = "LLC", CostCenterCode = "40-01-01", AccountingAccountCode = "5-01-08-000-0000" },
                            new() { MovementTypeName = "On Call Time Worked",CompanyId = "LLC", CostCenterCode = "40-01-01", AccountingAccountCode = "5-01-09-000-0000" },

                            // OCE
                            new() { MovementTypeName = "Balance Program",   CompanyId = "OCE", CostCenterCode = "10-02-04", AccountingAccountCode = "3-02-01-000-000" },
                            new() { MovementTypeName = "Bonusly Rewards",   CompanyId = "OCE", CostCenterCode = "10-02-04", AccountingAccountCode = "3-02-02-000-000" },
                            new() { MovementTypeName = "Holidays",          CompanyId = "OCE", CostCenterCode = "40-01-01", AccountingAccountCode = "5-01-06-000-000" },
                            new() { MovementTypeName = "Interviews",        CompanyId = "OCE", CostCenterCode = "30-01-01", AccountingAccountCode = "6-01-01-005-000" },
                            new() { MovementTypeName = "Normal Hours",      CompanyId = "OCE", CostCenterCode = "40-01-01", AccountingAccountCode = "5-01-01-000-000" },
                            new() { MovementTypeName = "Oceans Challenge",  CompanyId = "OCE", CostCenterCode = "10-02-04", AccountingAccountCode = "6-01-03-005-000" },
                            new() { MovementTypeName = "On Call Flate Rate",CompanyId = "OCE", CostCenterCode = "40-01-01", AccountingAccountCode = "5-01-16-000-000" },
                            new() { MovementTypeName = "On Call Time Worked",CompanyId = "OCE", CostCenterCode = "40-01-01", AccountingAccountCode = "5-01-22-000-000" }
                        }
                    });

                    // 1) Insert positions first
                    var newPositions = consultantPositionsList
                        .Select(p => new ConsultantPosition
                        {
                            Name = p.Name,
                            IsAdministrative = p.IsAdministrative
                        })
                        .ToList();

                    await _db.CONSULTANT_POSITIONS.AddRangeAsync(newPositions);
                    await _db.SaveChangesAsync();

                    // 2) Build lookups
                    var posNameToId = await _db.CONSULTANT_POSITIONS
                        .AsNoTracking()
                        .Where(p => consultantPositionsList.Select(x => x.Name).Contains(p.Name))
                        .ToDictionaryAsync(p => p.Name, p => p.ConsultantPositionId);

                    var movTypeByName = await _db.REPORTING_MY_TIME_MOVEMENT_TYPES
                        .AsNoTracking()
                        .ToDictionaryAsync(m => m.Name, m => m.MovementTypeId);

                    // 3) Build accounting configurations for all positions
                    var posAccConfigs = new List<ConsultantPositionAccountingConfiguration>();

                    foreach (var pos in consultantPositionsList)
                    {
                        if (!posNameToId.TryGetValue(pos.Name, out var positionId))
                            throw new InvalidOperationException($"Position '{pos.Name}' not found after insert.");

                        foreach (var aConfig in pos.AccountingConfig)
                        {
                            var ccId = CC(aConfig.CostCenterCode, aConfig.CompanyId);
                            var aaId = AA(aConfig.AccountingAccountCode, aConfig.CompanyId);

                            if (!movTypeByName.TryGetValue(aConfig.MovementTypeName, out var movId))
                                throw new InvalidOperationException($"Movement type '{aConfig.MovementTypeName}' not found.");

                            posAccConfigs.Add(new ConsultantPositionAccountingConfiguration
                            {
                                CompanyId = aConfig.CompanyId,
                                CostCenterId = ccId,
                                AccountingAccountId = aaId,
                                MovementTypeId = movId,
                                PositionId = positionId
                            });
                        }
                    }

                    // Prevent duplicates if the initializer runs multiple times unexpectedly
                    var existingPosConfigs = await _db.CONSULTANT_POSITIONS_ACCOUNTING_CONFIGURATION
                        .AsNoTracking()
                        .Select(x => new { x.PositionId, x.MovementTypeId, x.CompanyId, x.CostCenterId, x.AccountingAccountId })
                        .ToListAsync();
                    var existingPosConfigKeys = existingPosConfigs
                        .Select(x => (x.PositionId, x.MovementTypeId, x.CompanyId, x.CostCenterId, x.AccountingAccountId))
                        .ToHashSet();

                    var posAccConfigsToInsert = posAccConfigs
                        .Where(x => !existingPosConfigKeys.Contains((x.PositionId, x.MovementTypeId, x.CompanyId, x.CostCenterId, x.AccountingAccountId)))
                        .ToList();

                    if (posAccConfigsToInsert.Count > 0)
                        await _db.CONSULTANT_POSITIONS_ACCOUNTING_CONFIGURATION.AddRangeAsync(posAccConfigsToInsert);

                    await _db.SaveChangesAsync();
                }

                // ----------------- TRANSACTION TYPES --------------------------------

                var transactionTypesList = new List<TransactionType>
                {
                    new() { Name = "Debit" },
                    new() { Name = "Credit" }
                };

                var existingTxTypes = (await _db.TRANSACTION_TYPES
                    .AsNoTracking()
                    .Select(x => x.Name)
                    .ToListAsync())
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                var txTypesToInsert = transactionTypesList
                    .Where(t => !existingTxTypes.Contains(t.Name))
                    .ToList();

                if (txTypesToInsert.Count > 0)
                    await _db.TRANSACTION_TYPES.AddRangeAsync(txTypesToInsert);

                await _db.SaveChangesAsync();

                // ----------------- DOCUMENT TYPES --------------------------------

                var debitTypeId = await _db.TRANSACTION_TYPES
                     .AsNoTracking()
                     .Where(t => t.Name == "Debit")
                     .Select(t => t.TransactionTypeId)
                     .FirstOrDefaultAsync();
                if (debitTypeId == 0) throw new InvalidOperationException("TransactionType 'Debit' not found.");

                var creditTypeId = await _db.TRANSACTION_TYPES
                    .AsNoTracking()
                    .Where(t => t.Name == "Credit")
                    .Select(t => t.TransactionTypeId)
                    .FirstOrDefaultAsync();
                if (creditTypeId == 0) throw new InvalidOperationException("TransactionType 'Credit' not found.");

                var documentTypesList = new List<DocumentType>
                {
                    new() { DocumentTypeId = "FAC", TransactionTypeId = debitTypeId,  Description = "Invoice" },
                    new() { DocumentTypeId = "I/C", TransactionTypeId = debitTypeId,  Description = "Current Interest" },
                    new() { DocumentTypeId = "INT", TransactionTypeId = debitTypeId,  Description = "Late Payment Interest" },
                    new() { DocumentTypeId = "L/C", TransactionTypeId = debitTypeId,  Description = "Bill of Exchange" },
                    new() { DocumentTypeId = "N/D", TransactionTypeId = debitTypeId,  Description = "Debit Note" },
                    new() { DocumentTypeId = "O/D", TransactionTypeId = debitTypeId,  Description = "Other Debit" },
                    new() { DocumentTypeId = "PAG", TransactionTypeId = debitTypeId,  Description = "Promissory Note" },
                    new() { DocumentTypeId = "DEP", TransactionTypeId = creditTypeId, Description = "Deposit" },
                    new() { DocumentTypeId = "N/C", TransactionTypeId = creditTypeId, Description = "Credit Note" },
                    new() { DocumentTypeId = "O/C", TransactionTypeId = creditTypeId, Description = "Other Credit" },
                    new() { DocumentTypeId = "TEF", TransactionTypeId = creditTypeId, Description = "Transfer" }
                };

                var existingDocTypeIds = (await _db.DOCUMENTS_TYPES
                    .AsNoTracking()
                    .Select(x => x.DocumentTypeId)
                    .ToListAsync())
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                var docTypesToInsert = documentTypesList
                    .Where(d => !existingDocTypeIds.Contains(d.DocumentTypeId))
                    .ToList();

                if (docTypesToInsert.Count > 0)
                    await _db.DOCUMENTS_TYPES.AddRangeAsync(docTypesToInsert);

                await _db.SaveChangesAsync();

                // ----------------- GLOBAL CONSECUTIVES --------------------------------

                var globalConsecutivesList = new List<GlobalConsecutive>
                {
                    new() { Name = "JOURNAL_CXP", ConsecutiveNumber = 0, CompanyId = "OCE" },
                    new() { Name = "JOURNAL_CXP", ConsecutiveNumber = 0, CompanyId = "LLC" },
                    new() { Name = "FAC",         ConsecutiveNumber = 0, CompanyId = "OCE" },
                    new() { Name = "FAC",         ConsecutiveNumber = 0, CompanyId = "LLC" },
                    new() { Name = "I/C",         ConsecutiveNumber = 0, CompanyId = "OCE" },
                    new() { Name = "I/C",         ConsecutiveNumber = 0, CompanyId = "LLC" },
                    new() { Name = "INT",         ConsecutiveNumber = 0, CompanyId = "OCE" },
                    new() { Name = "INT",         ConsecutiveNumber = 0, CompanyId = "LLC" },
                    new() { Name = "L/C",         ConsecutiveNumber = 0, CompanyId = "OCE" },
                    new() { Name = "L/C",         ConsecutiveNumber = 0, CompanyId = "LLC" },
                    new() { Name = "N/D",         ConsecutiveNumber = 0, CompanyId = "OCE" },
                    new() { Name = "N/D",         ConsecutiveNumber = 0, CompanyId = "LLC" },
                    new() { Name = "O/D",         ConsecutiveNumber = 0, CompanyId = "OCE" },
                    new() { Name = "O/D",         ConsecutiveNumber = 0, CompanyId = "LLC" },
                    new() { Name = "PAG",         ConsecutiveNumber = 0, CompanyId = "OCE" },
                    new() { Name = "PAG",         ConsecutiveNumber = 0, CompanyId = "LLC" },
                    new() { Name = "PRODUCTS",    ConsecutiveNumber = 3, CompanyId = "OCE" }
                };

                var existingConsecutives = await _db.GLOBAL_CONSECUTIVES
                    .AsNoTracking()
                    .Select(x => new { x.Name, x.CompanyId })
                    .ToListAsync();
                var existingConsecutiveKeys = existingConsecutives
                    .Select(x => (x.Name, x.CompanyId))
                    .ToHashSet();

                var consecutivesToInsert = globalConsecutivesList
                    .Where(g => !existingConsecutiveKeys.Contains((g.Name, g.CompanyId)))
                    .ToList();

                if (consecutivesToInsert.Count > 0)
                    await _db.GLOBAL_CONSECUTIVES.AddRangeAsync(consecutivesToInsert);

                await _db.SaveChangesAsync();

                // ----------------- DEFAULT CLIENT FOR TESTING --------------------------------
                var anyClient = await _db.CLIENT.AnyAsync();
                if (!anyClient)
                {
                    Client client = new()
                    {
                        Name = "Client for testing",
                        ClientCode = "OCE_001",
                        Alias = "Client Test",
                        AdmissionDate = DateTime.Now,
                        PaymentCondition = "7",
                        Discount = 0,
                        IsActive = "S",
                        ClientCategory = "EXT",
                        CreationDate = DateTime.Now,
                        CompanyId = "OCE",
                        LatePaymentFee = 0,
                        AllowSentLatePaymentNotifications = true
                    };
                    await _db.CLIENT.AddAsync(client);
                    await _db.SaveChangesAsync();
                }
                // ----------------- DEFAULT CLIENT FOR ADMINISTRATIVE CONSULTANTS --------------------------------

                var existsOceAdmin = await _db.CLIENT.AsNoTracking().FirstOrDefaultAsync(x => x.Name == "Oceans Code Experts");
                if (existsOceAdmin == null)
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
                    await _db.CLIENT.AddAsync(client);
                    await _db.SaveChangesAsync();
                }

                // ----------------- PARTNERS --------------------------------

                var partnersList = new List<Partner>
                {
                    new()
                    {
                        Name = "Global Business",
                        Contact = "Yeanett Russo",
                        ContactOccupation = "Admin and Finances",
                        ContactEmail = "yeanett.russo@gbitcorp.com",
                        Phone = "(+507) 310 2673",
                        AdmissionDate = DateTime.Parse("2022-02-07"),
                        IsActive = true,
                        CreationDate = DateTime.UtcNow,
                        CompanyId = "OCE",
                        IdCountry = "PAN"
                    },
                    new()
                    {
                        Name = "Syntepro",
                        Contact = "Andrés Ureña",
                        ContactOccupation = "Accountant",
                        ContactEmail = "andres.urena@gruposyntepro.com",
                        Phone = "(+506) 2101 8823",
                        AdmissionDate = DateTime.Parse("2021-06-01"),
                        IsActive = true,
                        CreationDate = DateTime.UtcNow,
                        CompanyId = "OCE",
                        IdCountry = "CRI"
                    }
                };

                var existingPartners = (await _db.PARTNERS
                    .AsNoTracking()
                    .Select(x => x.Name)
                    .ToListAsync())
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                var partnersToInsert = partnersList
                    .Where(p => !existingPartners.Contains(p.Name))
                    .ToList();

                if (partnersToInsert.Count > 0)
                    await _db.PARTNERS.AddRangeAsync(partnersToInsert);

                await _db.SaveChangesAsync();

                // ----------------- PAYMENT METHODS --------------------------------

                List<PaymentMethod> paymentMethodsList = new List<PaymentMethod>
                {
                    new PaymentMethod { Name = "Bac Credomatic different from Panamá (Ameritransfer)", CompanyId = "OCE" },
                    new PaymentMethod { Name = "Other banks (International Transfer)", CompanyId = "OCE" },
                    new PaymentMethod { Name = "Payoneer", CompanyId = "OCE" },
                    new PaymentMethod { Name = "Banco General (Panamá)", CompanyId = "OCE" },
                    new PaymentMethod { Name = "Bac Credomatic (Panamá)", CompanyId = "OCE" },
                    new PaymentMethod { Name = "Mercury", CompanyId = "LLC" },
                    new PaymentMethod { Name = "Wise", CompanyId = "LLC" },
                    new PaymentMethod { Name = "Bac Credomatic Costa Rica (Bac CR to Bac CR)", CompanyId = "OCE" },
                    new PaymentMethod { Name = "USA local transfer", CompanyId = "LLC" }
                };

                var existingPMs = (await _db.PAYMENT_METHODS
                     .AsNoTracking()
                     .Select(x => x.Name)
                     .ToListAsync())
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                var pmsToInsert = paymentMethodsList
                    .Where(pm => !existingPMs.Contains(pm.Name))
                    .ToList();

                if (pmsToInsert.Count > 0)
                    await _db.PAYMENT_METHODS.AddRangeAsync(pmsToInsert);

                await _db.SaveChangesAsync();

                // ----------------- DEFAULT BANK ACCOUNTS --------------------------------

                List<BankAccount> bankAccountsList = new List<BankAccount>
                {
                    new BankAccount { BankAccountCode = "113439285", BankAccountName = "Cta # 113439285  BAC CREDOMATIC PANAMA", IsActive = "S", CompanyId = "OCE" },
                    new BankAccount { BankAccountCode = "202218366303", BankAccountName = "Cta #202218366303 Mercury", IsActive = "S", CompanyId = "LLC" },
                };

                var existingBA = (await _db.BANK_ACCOUNTS
                    .AsNoTracking()
                    .Select(x => x.BankAccountCode)
                    .ToListAsync())
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                var baToInsert = bankAccountsList
                    .Where(b => !existingBA.Contains(b.BankAccountCode))
                    .ToList();

                if (baToInsert.Count > 0)
                    await _db.BANK_ACCOUNTS.AddRangeAsync(baToInsert);

                await _db.SaveChangesAsync();

                // ----------------- PAYMENT METHOD BANK ACCOUNTS --------------------------------

                var paymentMethods = await _db.PAYMENT_METHODS.AsNoTracking().ToListAsync();
                var bankAccounts = await _db.BANK_ACCOUNTS.AsNoTracking().ToListAsync();

                int PM(string name) =>
                    paymentMethods.FirstOrDefault(x => x.Name == name)?.PaymentMethodId
                    ?? throw new InvalidOperationException($"PaymentMethod '{name}' not found.");

                int BA(string code) =>
                    bankAccounts.FirstOrDefault(x => x.BankAccountCode == code)?.BankAccountId
                    ?? throw new InvalidOperationException($"BankAccount '{code}' not found.");

                var paymentMethodBankAccountList = new List<PaymentMethodBankAccount>
                {
                    new() { PaymentMethodId = PM("Bac Credomatic different from Panamá (Ameritransfer)"), BankAccountId = BA("113439285"),     IsDefault = true },
                    new() { PaymentMethodId = PM("Other banks (International Transfer)"), BankAccountId = BA("113439285"),     IsDefault = true },
                    new() { PaymentMethodId = PM("Banco General (Panamá)"), BankAccountId = BA("113439285"),     IsDefault = true },
                    new() { PaymentMethodId = PM("Bac Credomatic (Panamá)"), BankAccountId = BA("113439285"),     IsDefault = true },
                    new() { PaymentMethodId = PM("Mercury"), BankAccountId = BA("202218366303"),  IsDefault = true }
                };

                var existingPMBA = await _db.PAYMENT_METHOD_AND_BANK_ACCOUNTS
                    .AsNoTracking()
                    .Select(x => new { x.PaymentMethodId, x.BankAccountId })
                    .ToListAsync();
                var existingPMBAKeys = existingPMBA
                    .Select(x => (x.PaymentMethodId, x.BankAccountId))
                    .ToHashSet();

                var pmbaToInsert = paymentMethodBankAccountList
                    .Where(x => !existingPMBAKeys.Contains((x.PaymentMethodId, x.BankAccountId)))
                    .ToList();

                if (pmbaToInsert.Count > 0)
                    await _db.PAYMENT_METHOD_AND_BANK_ACCOUNTS.AddRangeAsync(pmbaToInsert);

                await _db.SaveChangesAsync();

                // ----------------- PRODUCTS BY DEFAULT --------------------------------

                var productsList = new List<Product>
                {
                    new() { Name = "Hours of professional services",              ProductCode = "PR_0000001", Alias = "Hours of professional services" },
                    new() { Name = "On Call Flate Rate",                          ProductCode = "PR_0000002", Alias = "On Call Flate Rate" },
                    new() { Name = "On Call Time Worked",                         ProductCode = "PR_0000003", Alias = "On Call Time Worked" },
                    new() { Name = "Hours of professional services(Overtime)",    ProductCode = "PR_0000004", Alias = "Hours of professional services(Overtime)" }
                };

                var existingProducts = (await _db.PRODUCTS
                    .AsNoTracking()
                    .Select(x => x.Name)
                    .ToListAsync())
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                var productsToInsert = productsList
                    .Where(p => !existingProducts.Contains(p.Name))
                    .ToList();

                if (productsToInsert.Count > 0)
                    await _db.PRODUCTS.AddRangeAsync(productsToInsert);

                await _db.SaveChangesAsync();


                // ----------------- TRANSACTION STATUSES --------------------------------

                var transactionStatusesList = new List<TransactionStatus>
                {
                    new () { Name = "No actions" },
                    new () { Name = "Waiting to be approved" },
                    new () { Name = "Approved" },
                    new () { Name = "Rejected" },
                    new () { Name = "Sent to be paid" },
                    new () { Name = "Paid" },
                    new () { Name = "Pending Accounting" },
                    new () { Name = "Accounted" },
                    new () { Name = "Updated - Pending Review" }
                };

                var existingStatuses = (await _db.TRANSACTION_STATUSES
                    .AsNoTracking()
                    .Select(x => x.Name)
                    .ToListAsync())
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                var statusesToInsert = transactionStatusesList
                    .Where(s => !existingStatuses.Contains(s.Name))
                    .ToList();

                if (statusesToInsert.Count > 0)
                    await _db.TRANSACTION_STATUSES.AddRangeAsync(statusesToInsert);

                await _db.SaveChangesAsync();

                //---------------SYSTEM ARCHITECTURE------------------------------
                // ----------------- SYSTEM AREAS --------------------------------

                var systemAreasList = new List<SystemArea>
                {
                    new () { Name = "Admin Center" },
                    new () { Name = "Finances" },
                    new () { Name = "General" },
                    new () { Name = "Tracking Tool" },
                    new () { Name = "Dashboard" },
                    new () { Name = "My Account" },
                    new () { Name = "Account Management" },
                    new () { Name = "Recruiting" }
                };

                var existingAreas = (await _db.SYSTEM_AREAS
                    .AsNoTracking()
                    .Select(x => x.Name)
                    .ToListAsync())
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                var areasToInsert = systemAreasList
                    .Where(a => !existingAreas.Contains(a.Name))
                    .ToList();

                if (areasToInsert.Count > 0)
                    await _db.SYSTEM_AREAS.AddRangeAsync(areasToInsert);

                await _db.SaveChangesAsync();

                // ----------------- SYSTEM SUB AREAS --------------------------------

                var areasDict = await _db.SYSTEM_AREAS
                    .AsNoTracking()
                    .ToDictionaryAsync(a => a.Name, a => a.SystemAreaId);

                int AREA(string name) =>
                    areasDict.TryGetValue(name, out var id) ? id : throw new InvalidOperationException($"SystemArea '{name}' not found.");

                var systemSubAreasList = new List<SystemSubArea>
                {
                    new() { SystemAreaId = AREA("Admin Center"),       Name = "Actualizar Datos desde Softland" },
                    new() { SystemAreaId = AREA("Admin Center"),       Name = "Roles y Permisos de Usuarios" },
                    new() { SystemAreaId = AREA("Admin Center"),       Name = "Consultant Positions Accounting Configuration" },

                    new() { SystemAreaId = AREA("Finances"),           Name = "Cuentas Por Cobrar" },
                    new() { SystemAreaId = AREA("Finances"),           Name = "Consultant Payment Debits & Credits" },
                    new() { SystemAreaId = AREA("Finances"),           Name = "Payment Sheets" },
                    new() { SystemAreaId = AREA("Finances"),           Name = "Export Accounting Data" },
                    new() { SystemAreaId = AREA("Finances"),           Name = "Calculadora Financiera" },

                    new() { SystemAreaId = AREA("General"),            Name = "Consultants" },
                    new() { SystemAreaId = AREA("General"),            Name = "Consultant Reimbursed Benefits" },
                    new() { SystemAreaId = AREA("General"),            Name = "Holidays" },

                    new() { SystemAreaId = AREA("Tracking Tool"),      Name = "Reporting My Time" },
                    new() { SystemAreaId = AREA("Tracking Tool"),      Name = "General Reports" },
                    new() { SystemAreaId = AREA("Tracking Tool"),      Name = "My Reports History" },

                    new() { SystemAreaId = AREA("Dashboard"),          Name = "Dashboard" },
                    new() { SystemAreaId = AREA("My Account"),         Name = "Mi Cuenta" },

                    new() { SystemAreaId = AREA("Account Management"), Name = "Clients" },
                    new() { SystemAreaId = AREA("Account Management"), Name = "Projects" },

                    new() { SystemAreaId = AREA("Recruiting"),         Name = "Interviews" }
                };

                var existingSubAreas = (await _db.SYSTEM_SUB_AREAS
                    .AsNoTracking()
                    .Select(x => x.Name)
                    .ToListAsync())
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                var subAreasToInsert = systemSubAreasList
                    .Where(sa => !existingSubAreas.Contains(sa.Name))
                    .ToList();

                if (subAreasToInsert.Count > 0)
                    await _db.SYSTEM_SUB_AREAS.AddRangeAsync(subAreasToInsert);

                await _db.SaveChangesAsync();

                // ----------------- CLAIMS --------------------------------

                var subAreasDict = await _db.SYSTEM_SUB_AREAS
                     .AsNoTracking()
                     .ToDictionaryAsync(x => x.Name, x => x.SystemSubAreaId);

                int SUBAREA(string name) =>
                    subAreasDict.TryGetValue(name, out var id) ? id : throw new InvalidOperationException($"SystemSubArea '{name}' not found.");

                var systemClaimsList = new List<ApplicationSystemClaim>();

                // ADMIN CENTER
                systemClaimsList.Add(new ApplicationSystemClaim
                {
                    ClaimType = AdminCenterClaimsCD.Actualizar_Datos_Softland_ClaimType,
                    ClaimValue = AdminCenterClaimsCD.Actualizar_Datos_Softland_ClaimValue,
                    Description = "Acceso a poder actualizar los datos extraídos desde Softland",
                    SystemSubAreaId = SUBAREA("Actualizar Datos desde Softland")
                });
                systemClaimsList.Add(new ApplicationSystemClaim
                {
                    ClaimType = AdminCenterClaimsCD.Roles_Permisos_Usuarios_ClaimType,
                    ClaimValue = AdminCenterClaimsCD.Roles_Permisos_Usuarios_ClaimValue,
                    Description = "Acceso a ver y editar los roles y permisos de usuarios",
                    SystemSubAreaId = SUBAREA("Roles y Permisos de Usuarios")
                });
                systemClaimsList.Add(new ApplicationSystemClaim
                {
                    ClaimType = ConsultantPositionsClaimsCD.Manage_Consultant_Positions_ClaimType,
                    ClaimValue = ConsultantPositionsClaimsCD.Manage_Consultant_Positions_ClaimValue,
                    Description = "Have access to manage the consultant positions accounting configuration",
                    SystemSubAreaId = SUBAREA("Consultant Positions Accounting Configuration")
                });
                // NOTES FOR ADMIN CENTER PERMISSIONS:
                // Add every permission to the AnyOfPoliciesAdminCenterRequirementHandler

                // FINANCES
                systemClaimsList.Add(new ApplicationSystemClaim
                {
                    ClaimType = FinancesClaimsCD.Accounts_Receivable_ClaimType,
                    ClaimValue = FinancesClaimsCD.Accounts_Receivable_ClaimValue,
                    Description = "Acceso a la sección de cuentas por cobrar",
                    SystemSubAreaId = SUBAREA("Cuentas Por Cobrar")
                });
                systemClaimsList.Add(new ApplicationSystemClaim
                {
                    ClaimType = FinancesClaimsCD.Financial_Calculator_ClaimType,
                    ClaimValue = FinancesClaimsCD.Financial_Calculator_ClaimValue,
                    Description = "Acceso básico a la calculadora financiera",
                    SystemSubAreaId = SUBAREA("Calculadora Financiera")
                });
                systemClaimsList.Add(new ApplicationSystemClaim
                {
                    ClaimType = FinancesClaimsCD.Financial_Calculator_BasicConfig_ClaimType,
                    ClaimValue = FinancesClaimsCD.Financial_Calculator_BasicConfig_ClaimValue,
                    Description = "Acceso a la configuración básica de la calculadora financiera",
                    SystemSubAreaId = SUBAREA("Calculadora Financiera")
                });
                systemClaimsList.Add(new ApplicationSystemClaim
                {
                    ClaimType = FinancesClaimsCD.Financial_Calculator_AdvancedConfig_ClaimType,
                    ClaimValue = FinancesClaimsCD.Financial_Calculator_AdvancedConfig_ClaimValue,
                    Description = "Acceso avanzado en la configuración de la calculadora (editar los porcentages de utilidad, porcentages de riesgo, etc.)",
                    SystemSubAreaId = SUBAREA("Calculadora Financiera")
                });
                systemClaimsList.Add(new ApplicationSystemClaim
                {
                    ClaimType = FinancesClaimsCD.Financial_Calculator_Profit_And_Details_ClaimType,
                    ClaimValue = FinancesClaimsCD.Financial_Calculator_Profit_And_Details_ClaimValue,
                    Description = "Acceso a ver las utilidades de los resultados de la calculadora financiera y a más detalles.",
                    SystemSubAreaId = SUBAREA("Calculadora Financiera")
                });
                systemClaimsList.Add(new ApplicationSystemClaim
                {
                    ClaimType = FinancesClaimsCD.Financial_Calculator_Remove_Expenses_And_Costs_And_Edit_Vacations_ClaimType,
                    ClaimValue = FinancesClaimsCD.Financial_Calculator_Remove_Expenses_And_Costs_And_Edit_Vacations_ClaimValue,
                    Description = "Acceso a editar la opcion de vacaciones y remover gastos y costos para no ser tomados en cuenta en el calculo de la calculadora financiera.",
                    SystemSubAreaId = SUBAREA("Calculadora Financiera")
                });

                systemClaimsList.Add(new ApplicationSystemClaim
                {
                    ClaimType = FinancesClaimsCD.Manage_Payment_Debits_Credits_ClaimType,
                    ClaimValue = FinancesClaimsCD.Manage_Payment_Debits_Credits_ClaimValue,
                    Description = "Have access to manage payment debits and credits of payments to consultants.",
                    SystemSubAreaId = SUBAREA("Consultant Payment Debits & Credits")
                });

                systemClaimsList.Add(new ApplicationSystemClaim
                {
                    ClaimType = FinancesClaimsCD.Manage_Basic_Payment_Sheets_ClaimType,
                    ClaimValue = FinancesClaimsCD.Manage_Basic_Payment_Sheets_ClaimValue,
                    Description = "Have access to manage the basics of Payment Sheets.",
                    SystemSubAreaId = SUBAREA("Payment Sheets")
                });

                systemClaimsList.Add(new ApplicationSystemClaim
                {
                    ClaimType = FinancesClaimsCD.Access_Export_Accounting_Data_ClaimType,
                    ClaimValue = FinancesClaimsCD.Access_Export_Accounting_Data_ClaimValue,
                    Description = "Have access to export the accounting data.",
                    SystemSubAreaId = SUBAREA("Export Accounting Data")
                });

                // GENERAL - CONSULTANTS
                systemClaimsList.Add(new ApplicationSystemClaim
                {
                    ClaimType = ConsultantsClaimsCD.Consultants_Page_ClaimType,
                    ClaimValue = ConsultantsClaimsCD.Consultants_Page_ClaimValue,
                    Description = "Access to manage only Computer Consultants (Developers, QAs...)",
                    SystemSubAreaId = SUBAREA("Consultants")
                });
                systemClaimsList.Add(new ApplicationSystemClaim
                {
                    ClaimType = ConsultantsClaimsCD.Manage_Administrative_Consultants_ClaimType,
                    ClaimValue = ConsultantsClaimsCD.Manage_Administrative_Consultants_ClaimValue,
                    Description = "Access to manage all consultants, including Administrative Consultants",
                    SystemSubAreaId = SUBAREA("Consultants")
                });

                // GENERAL - HOLIDAYS
                systemClaimsList.Add(new ApplicationSystemClaim
                {
                    ClaimType = HolidaysClaimsCD.Holidays_Page_ClaimType,
                    ClaimValue = HolidaysClaimsCD.Holidays_Page_ClaimValue,
                    Description = "Acceso básico para ver todos los holidays",
                    SystemSubAreaId = SUBAREA("Holidays")
                });

                // GENERAL - CONSULTANT REIMBURSED BENEFITS
                systemClaimsList.Add(new ApplicationSystemClaim
                {
                    ClaimType = ConsultantReimbursedBenefitsClaimsCD.Manage_Consultant_Reimbursed_Benefits_ClaimType,
                    ClaimValue = ConsultantReimbursedBenefitsClaimsCD.Manage_Consultant_Reimbursed_Benefits_ClaimValue,
                    Description = "Access to manage the consultant reimbursed benefits to pay.",
                    SystemSubAreaId = SUBAREA("Consultant Reimbursed Benefits")
                });

                // HOURS TRACKING TOOL
                systemClaimsList.Add(new ApplicationSystemClaim
                {
                    ClaimType = TrackingToolClaimsCD.Reporting_My_Time_Basic_Access_ClaimType,
                    ClaimValue = TrackingToolClaimsCD.Reporting_My_Time_Basic_Access_ClaimValue,
                    Description = "Basic access to report their time",
                    SystemSubAreaId = SUBAREA("Reporting My Time")
                });
                systemClaimsList.Add(new ApplicationSystemClaim
                {
                    ClaimType = TrackingToolClaimsCD.General_Reports_ClaimType,
                    ClaimValue = TrackingToolClaimsCD.General_Reports_ClaimValue,
                    Description = "Access to view hours report for all consultants",
                    SystemSubAreaId = SUBAREA("Reporting My Time")
                });

                // PROJECT MANAGEMENT - CLIENTS
                systemClaimsList.Add(new ApplicationSystemClaim
                {
                    ClaimType = ClientsClaimsCD.Clients_Page_ClaimType,
                    ClaimValue = ClientsClaimsCD.Clients_Page_ClaimValue,
                    Description = "Acces to view the Clients list",
                    SystemSubAreaId = SUBAREA("Clients")
                });

                // PROJECT MANAGEMENT - PROJECTS
                systemClaimsList.Add(new ApplicationSystemClaim
                {
                    ClaimType = ProjectsClaimsCD.Projects_Page_ClaimType,
                    ClaimValue = ProjectsClaimsCD.Projects_Page_ClaimValue,
                    Description = "Acces to view the Projects list",
                    SystemSubAreaId = SUBAREA("Projects")
                });

                // RECRUITING - INTERVIEWS
                systemClaimsList.Add(new ApplicationSystemClaim
                {
                    ClaimType = InterviewsClaimsCD.Manage_Interviews_Page_ClaimType,
                    ClaimValue = InterviewsClaimsCD.Manage_Interviews_ClaimValue,
                    Description = "Acces to manage Interviews",
                    SystemSubAreaId = SUBAREA("Interviews")
                });

                var existingSystemClaims = await _db.APPLICATION_SYSTEM_CLAIMS
                    .AsNoTracking()
                    .Select(x => new { x.ClaimType, x.ClaimValue })
                    .ToListAsync();
                var existingSystemClaimKeys = existingSystemClaims
                    .Select(x => (x.ClaimType, x.ClaimValue))
                    .ToHashSet();

                var claimsToInsert = systemClaimsList
                    .Where(c => !existingSystemClaimKeys.Contains((c.ClaimType, c.ClaimValue)))
                    .Select(c => new ApplicationSystemClaim
                    {
                        ClaimType = c.ClaimType,
                        ClaimValue = c.ClaimValue,
                        Description = c.Description,
                        SystemSubAreaId = c.SystemSubAreaId
                    }).ToList();

                if (claimsToInsert.Count > 0)
                    await _db.APPLICATION_SYSTEM_CLAIMS.AddRangeAsync(claimsToInsert);

                await _db.SaveChangesAsync();

                // ----------------- USER ROLES --------------------------------

                var rolesList = new List<IdentityRole>
                {
                    new () { Name = SD.Role_User_Master },
                    new () { Name = SD.Role_User_Admin },
                    new () { Name = SD.Role_User_Computer_Consultant }
                };

                foreach (var role in rolesList)
                {
                    var exists = await _roleManager.FindByNameAsync(role.Name);
                    if (exists == null)
                    {
                        var result = await _roleManager.CreateAsync(new IdentityRole(role.Name));
                        if (!result.Succeeded)
                            throw new InvalidOperationException($"Failed to create role '{role.Name}': {string.Join("; ", result.Errors.Select(e => e.Description))}");
                    }
                }

                // ----------------- CREATE DEFAULT MASTER USER --------------------------------

                var masterUsers = await _userManager.GetUsersInRoleAsync(SD.Role_User_Master);
                if (masterUsers == null || !masterUsers.Any())
                {
                    var masterUserEmail = _config["MasterUserEmailENV"];
                    var masterUserPass = _config["MasterUserPassENV"];

                    if (string.IsNullOrWhiteSpace(masterUserEmail) || string.IsNullOrWhiteSpace(masterUserPass))
                        throw new InvalidOperationException("Master user credentials (MasterUserEmailENV / MasterUserPassENV) are not configured.");

                    var existingUser = await _userManager.FindByEmailAsync(masterUserEmail);

                    if (existingUser == null)
                    {
                        var userCategoryAdministrative = await _db.UserCategories
                            .AsNoTracking()
                            .FirstOrDefaultAsync(x => x.Name == "Administrative")
                            ?? throw new InvalidOperationException("UserCategory 'Administrative' not found.");

                        var user = new ApplicationUser
                        {
                            UserName = masterUserEmail,
                            Email = masterUserEmail,
                            Name = _config["MasterUserNameEnv"],
                            LastName = _config["MasterUserLastNameEnv"],
                            IsActive = true,
                            DeactivationDate = null,
                            UserCategoryId = userCategoryAdministrative.UserCategoryId,
                            PhoneNumberConfirmed = false,
                            TwoFactorEnabled = false,
                            LockoutEnabled = true,
                            AccessFailedCount = 0,
                            TwoFactorRequired = true,
                            EmailConfirmed = true
                        };

                        var createUserResult = await _userManager.CreateAsync(user, masterUserPass);
                        if (!createUserResult.Succeeded)
                            throw new InvalidOperationException($"Failed to create master user: {string.Join("; ", createUserResult.Errors.Select(e => e.Description))}");

                        // Refresh entity from context to update Name/LastName if needed (matches original)
                        var createdUser = await _db.AspNetUsers.FirstOrDefaultAsync(x => x.Email == masterUserEmail)
                            ?? throw new InvalidOperationException("Master user not retrievable from AspNetUsers after creation.");
                        createdUser.Name = "Master User Name";
                        createdUser.LastName = "Master User Last Name";

                        var addToRoleResult = await _userManager.AddToRoleAsync(createdUser, SD.Role_User_Master);
                        if (!addToRoleResult.Succeeded)
                            throw new InvalidOperationException($"Failed to assign Role_User_Master to user '{masterUserEmail}': {string.Join("; ", addToRoleResult.Errors.Select(e => e.Description))}");

                        await _db.SaveChangesAsync();

                        // Create Consultant Details and Active History
                        var defaultPaymentMethod = await _db.PAYMENT_METHODS
                            .AsNoTracking()
                            .FirstOrDefaultAsync(x => x.Name == "Bac Credomatic different from Panamá (Ameritransfer)")
                            ?? throw new InvalidOperationException("Default Payment Method 'Bac Credomatic different from Panamá (Ameritransfer)' not found.");

                        var consultantDetailToCreate = new ConsultantDetail
                        {
                            UserId = createdUser.Id,
                            CreationDate = DateTime.UtcNow,
                            IdCountry = "CRI",
                            UserCreatedBy = createdUser.Id,
                            CompanyId = "OCE",
                            PaymentMethodId = defaultPaymentMethod.PaymentMethodId,
                            PaymentPeriod = 1,
                            StartDate = DateTime.UtcNow,
                            WorkingModel = 1
                        };
                        var userActiveHistoryToCreate = new ApplicationUserActiveHistory
                        {
                            UserId = createdUser.Id,
                            IsActive = true,
                            ActionDate = DateTime.UtcNow,
                            UserIdActionedBy = createdUser.Id
                        };

                        await _db.CONSULTANT_DETAILS.AddAsync(consultantDetailToCreate);
                        await _db.UsersActiveHistory.AddAsync(userActiveHistoryToCreate);
                        await _db.SaveChangesAsync();
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"The user with the email: '{masterUserEmail}' already exists but is not a master user.");
                        Console.ResetColor();
                        throw new InvalidOperationException("User exists but is not assigned to Role_User_Master.");
                    }
                }
                // ----------------- ROLE CLAIMS --------------------------------

                var masterRole = await _roleManager.FindByNameAsync(SD.Role_User_Master)
                    ?? throw new InvalidOperationException("Master role not found.");
                var firstMasterUserId = await _db.UserRoles
                    .Where(ur => ur.RoleId == masterRole.Id)
                    .Select(ur => ur.UserId)
                    .FirstOrDefaultAsync();

                var systemClaimsAll = await _db.APPLICATION_SYSTEM_CLAIMS
                    .AsNoTracking()
                    .Select(c => new { c.ClaimType, c.ClaimValue })
                    .ToListAsync();

                var existingRoleClaims = await _db.RoleClaims
                    .AsNoTracking()
                    .Where(rc => rc.RoleId == masterRole.Id)
                    .Select(rc => new { rc.ClaimType, rc.ClaimValue })
                    .ToListAsync();

                var existingRoleClaimKeys = existingRoleClaims
                    .Select(x => (x.ClaimType, x.ClaimValue))
                    .ToHashSet();

                var roleClaimsToInsert = new List<ApplicationRoleClaim>();

                foreach (var sc in systemClaimsAll)
                {
                    if (!existingRoleClaimKeys.Contains((sc.ClaimType, sc.ClaimValue)))
                    {
                        roleClaimsToInsert.Add(new ApplicationRoleClaim
                        {
                            RoleId = masterRole.Id,
                            ClaimType = sc.ClaimType,
                            ClaimValue = sc.ClaimValue,
                            CreatedBy = firstMasterUserId,
                            CreationDate = DateTime.UtcNow
                        });
                    }
                }

                if (roleClaimsToInsert.Any())
                {
                    await _db.RoleClaims.AddRangeAsync(roleClaimsToInsert);
                    await _db.SaveChangesAsync();
                }

                await transaction.CommitAsync();

            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error during database initialization:");
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                Console.ResetColor();
                throw new InvalidOperationException("Error during database initialization.");
            }
            finally
            {
                _db.ChangeTracker.AutoDetectChangesEnabled = originalAutoDetect;
            }
        }
    }
}
