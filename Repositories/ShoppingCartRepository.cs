using System.Linq.Expressions;
using dotnetshop.Data;
using dotnetshop.Models;
using Microsoft.EntityFrameworkCore;

namespace dotnetshop.Repositories
{
    public class ShoppingCartRepository : Repository<ShoppingCart>, IShoppingCartRepository
    {
        private readonly AppDbContext _context;
        public ShoppingCartRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

    }
}