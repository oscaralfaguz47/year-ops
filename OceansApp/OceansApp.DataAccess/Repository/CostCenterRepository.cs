using Microsoft.EntityFrameworkCore;
using OceansApp.DataAccess.Data;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.Models;

namespace OceansApp.DataAccess.Repository
{
    public class CostCenterRepository : Repository<CostCenter>, ICostCenterRepository
    {
        private ApplicationDbContext _db;
        public CostCenterRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
        IEnumerable<CostCenter> ICostCenterRepository.GetCostCenterOfExpenses()
        {
            IEnumerable<CostCenter>? costCenter = GetAll(x => x.AcceptData == "S" && x.Description != "NO UTILIZAR" && (EF.Functions.Like(x.CostCenterCode, "10-02%")
            || EF.Functions.Like(x.CostCenterCode, "10-03%") || EF.Functions.Like(x.CostCenterCode, "20%") || EF.Functions.Like(x.CostCenterCode, "30%")
            || EF.Functions.Like(x.CostCenterCode, "40%") || EF.Functions.Like(x.CostCenterCode, "50%")));

                return costCenter;
        }
        public bool UpdateIfExistAddIfNot(CostCenter obj)
        {
            var existingCostCenter = GetFirstOrDefault(u => u.CostCenterCode == obj.CostCenterCode && u.CompanyId == obj.CompanyId);
            if (existingCostCenter == null)
            {
                _db.COST_CENTER.Add(obj);
                _db.SaveChanges();
                return true;
            }
            else
            {
                if (existingCostCenter.Description != obj.Description)
                {
                    existingCostCenter.Description = obj.Description;
                    existingCostCenter.AcceptData = obj.AcceptData;
                    return true;
                }
                return false;
            }
        }

        public void Update(CostCenter obj)
        {
            _db.COST_CENTER.Update(obj);
        }

    }
}
