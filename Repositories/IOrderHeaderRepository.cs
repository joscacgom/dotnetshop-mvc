using System.Linq.Expressions;
using dotnetshop.Models;

namespace dotnetshop.Repositories
{
    public interface IOrderHeaderRepository : IRepository<OrderHeader>
    {
        void UpdateStatus(int id, string orderStatus, string? paymentStatus = null);
        void UpdateStripePaymentId(int id, string sessionId ,string paymentIntentId);
    }
}