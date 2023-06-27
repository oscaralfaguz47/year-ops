
using OceansApp.DataAccess.Data;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.Models;

namespace OceansApp.DataAccess.Repository
{
    public class ProviderCategoryRepository : Repository<ProviderCategory>, IProviderCategoryRepository
    {
        private ApplicationDbContext _db;
        public ProviderCategoryRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
        
        public bool UpdateIfExistAddIfNot(ProviderCategory obj)
        {
            var existingCategory = GetFirstOrDefault(u => u.ProviderCategoryCode == obj.ProviderCategoryCode & u.CompanyId == obj.CompanyId);
            if (existingCategory == null)
            {
                _db.PROVIDER_CATEGORY.Add(obj);
                _db.SaveChanges();
                return true;
            }
            else
            {
                if (existingCategory.Description != obj.Description)
                {
                    existingCategory.Description = obj.Description;
                    return true;
                }
                return false;
            }
        }

        public void Update(ProviderCategory obj)
        {
            _db.PROVIDER_CATEGORY.Update(obj);
        }

    }
}
