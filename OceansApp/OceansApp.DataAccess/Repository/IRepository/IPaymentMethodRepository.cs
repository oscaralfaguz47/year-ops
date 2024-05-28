using OceansApp.Models.Models;

namespace OceansApp.DataAccess.Repository.IRepository
{
    public interface IPaymentMethodRepository : IRepository<PaymentMethod> 
    {
        void Update(PaymentMethod obj);

    }
}
