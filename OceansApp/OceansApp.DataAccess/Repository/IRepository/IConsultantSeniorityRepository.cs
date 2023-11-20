using OceansApp.Models.Models;
using OceansApp.Models.ViewModels.Components;

namespace OceansApp.DataAccess.Repository.IRepository
{
    public interface IConsultantSeniorityRepository : IRepository<ConsultantSeniority> 
    {
        Task<List<SelectVM>> GetSenioritisByRoleAsync(int roleId);
        void Update(ConsultantSeniority obj);
    }
}
