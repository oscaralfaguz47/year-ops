using Microsoft.EntityFrameworkCore;
using OceansApp.DataAccess.Data;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.Models;
using OceansApp.Models.ViewModels.Components;

namespace OceansApp.DataAccess.Repository
{
    public class ConsultantPositionRepository : Repository<ConsultantPosition>, IConsultantPositionRepository
    {
        private ApplicationDbContext _db;
        public ConsultantPositionRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public async Task<List<GetDataForSelectVM>> GetPositionsByIsAdministrative(bool isAdministrative)
        {
            IEnumerable<ConsultantPosition> positionsListFromDb = await _db.CONSULTANT_POSITIONS.Where(x => x.IsAdministrative == isAdministrative).ToListAsync();
            List<GetDataForSelectVM> positionsToReturn = new();
            foreach (var position in positionsListFromDb)
            {
                positionsToReturn.Add(new GetDataForSelectVM
                {
                    Value = position.ConsultantPositionId,
                    Text = position.Name 
                });
            }
            return positionsToReturn;
        }

        public void Update(ConsultantPosition obj)
        {
            _db.CONSULTANT_POSITIONS.Update(obj);
        }

    }
}
