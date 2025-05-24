using OceansApp.DataAccess.Data;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.Models;
namespace OceansApp.DataAccess.Repository
{
    public class ProductClientCompanyAccountingConfigForBillingRepository : Repository<ProductClientCompanyAccountingConfigForBilling>, IProductClientCompanyAccountingConfigForBillingRepository
    {
        private ApplicationDbContext _db;
        public ProductClientCompanyAccountingConfigForBillingRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }


    }
}
