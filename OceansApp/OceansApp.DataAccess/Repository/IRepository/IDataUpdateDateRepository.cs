using OceansApp.Models.Models;

namespace OceansApp.DataAccess.Repository.IRepository
{
    public interface IDataUpdateDateRepository : IRepository<DataUpdateDate> 
    {
        void Update(DataUpdateDate obj);
        Task<DataUpdateDate> GetLastDate();

    }
}
