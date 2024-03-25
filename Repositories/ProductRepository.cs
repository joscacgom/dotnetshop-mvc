using System.Linq.Expressions;
using dotnetshop.Data;
using dotnetshop.Models;
using Microsoft.EntityFrameworkCore;

namespace dotnetshop.Repositories
{
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        private readonly AppDbContext _context;
        public ProductRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

    }
}