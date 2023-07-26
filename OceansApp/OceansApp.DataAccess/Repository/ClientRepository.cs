using Microsoft.EntityFrameworkCore;
using OceansApp.DataAccess.Data;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.Models;

namespace OceansApp.DataAccess.Repository
{
    public class ClientRepository : Repository<Client>, IClientRepository
    {
        private ApplicationDbContext _db;
        public ClientRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
        public void Update(Client obj)
        {
            _db.CLIENT.Update(obj);
        }

        public bool UpdateIfExistAddIfNot(Client obj)
        {
            var existingClient = GetFirstOrDefault(u => u.ClientCode == obj.ClientCode && u.CompanyId == obj.CompanyId);
            if (existingClient == null)
            {
                _db.CLIENT.Add(obj);
                _db.SaveChanges();
                return true;
            }
            else
            {
                if (existingClient.DateLastUpdate != obj.DateLastUpdate)
                {
                    existingClient.Name = obj.Name;
                    existingClient.Alias = obj.Alias;
                    existingClient.Contact = obj.Contact;
                    existingClient.ContactOccupation = obj.ContactOccupation;
                    existingClient.Phone1 = obj.Phone1;
                    existingClient.Phone2 = obj.Phone2;
                    existingClient.AdmissionDate = obj.AdmissionDate;
                    existingClient.PaymentCondition = obj.PaymentCondition;
                    existingClient.Discount = obj.Discount;
                    existingClient.IsActive = obj.IsActive;
                    existingClient.ClientCategory = obj.ClientCategory;
                    existingClient.ClientClass = obj.ClientClass;
                    existingClient.Emails = obj.Emails;
                    existingClient.Notes = obj.Notes;
                    existingClient.DateLastUpdate = obj.DateLastUpdate;
                    existingClient.Address = obj.Address;
                    existingClient.CreationDate = obj.CreationDate;
                    existingClient.CompanyId = obj.CompanyId;
                    return true;
                }
                return false;
            }
        }

    }
}
