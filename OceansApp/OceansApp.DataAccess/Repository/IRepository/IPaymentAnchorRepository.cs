using OceansApp.Models.ViewModels.PaymentAnchor;

namespace OceansApp.DataAccess.Repository.IRepository
{
    /// <summary>
    /// Resolves the payment anchor for a consultant who has no active project assignment in the
    /// period being paid, by reading their assignment history. See docs/adr/0001 and
    /// <see cref="Services.PaymentAnchorResolver"/>.
    /// </summary>
    public interface IPaymentAnchorRepository
    {
        Task<PaymentAnchorResult?> ResolveAnchorAsync(int consultantId, DateTime periodEnd);
    }
}
