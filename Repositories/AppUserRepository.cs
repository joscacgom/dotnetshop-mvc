using System.Linq.Expressions;
using dotnetshop.Data;
using dotnetshop.Models;
using Microsoft.EntityFrameworkCore;

namespace dotnetshop.Repositories
{
    public class AppUserRepository : Repository<AppUser>, IAppUserRepository
    {
        private readonly AppDbContext _context;
        public AppUserRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

    }
}