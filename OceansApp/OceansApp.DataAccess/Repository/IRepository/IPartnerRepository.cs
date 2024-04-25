using OceansApp.Models.Models;
using System.Linq.Expressions;

namespace OceansApp.DataAccess.Repository.IRepository
{
    public interface IPartnerRepository : IRepository<Partner> 
    {
        Task<List<Partner>> GetAllAsync(Expression<Func<Partner, bool>>? predicate = null);
    }
}
