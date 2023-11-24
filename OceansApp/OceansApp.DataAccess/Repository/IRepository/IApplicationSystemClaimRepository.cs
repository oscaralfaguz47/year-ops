
using OceansApp.Models.Models;

namespace OceansApp.DataAccess.Repository.IRepository
{
    public interface IApplicationSystemClaimRepository : IRepository<ApplicationSystemClaim> 
    {
        void Update(ApplicationSystemClaim obj);
    }
}
