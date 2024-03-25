using System.Linq.Expressions;
using dotnetshop.Data;
using dotnetshop.Models;
using Microsoft.EntityFrameworkCore;

namespace dotnetshop.Repositories
{
    public class OrderDetailsRepository : Repository<OrderDetails>, IOrderDetailsRepository
    {
        private readonly AppDbContext _context;
        public OrderDetailsRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

    }
}