using System.Linq.Expressions;
using dotnetshop.Data;
using dotnetshop.Models;
using Microsoft.EntityFrameworkCore;

namespace dotnetshop.Repositories
{
    public class CompanyRepository : Repository<Company>, ICompanyRepository
    {
        private readonly AppDbContext _context;
        public CompanyRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

    }
}