using dotnetshop.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace dotnetshop.Data
{
    public class AppDbContext : IdentityDbContext<IdentityUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<AppUser> AppUsers { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<ShoppingCart> ShoppingCarts { get; set; }
        public DbSet<OrderHeader> OrderHeaders { get; set; }
        public DbSet<OrderDetails> OrderDetails { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            modelBuilder.Entity<Category>().HasData(new Category { CategoryId = 1, Name = "Books", DisplayOrder = 1 },
                new Category { CategoryId = 2, Name = "Sci-Fi", DisplayOrder = 2 },
                new Category { CategoryId = 3, Name = "Action", DisplayOrder = 3 },
                new Category { CategoryId = 4, Name = "History", DisplayOrder = 4 }
                );

            modelBuilder.Entity<Product>().HasData(new Product
            {
                ProductId = 1,
                Title = "The Hitchhiker's Guide to the Galaxy",
                Description = "The Hitchhiker's Guide to the Galaxy is a comedy science fiction series created by Douglas Adams. Originally a radio comedy broadcast on BBC Radio 4 in 1978, it was later adapted to other formats, including stage shows, novels, comic books, a 1981 TV series, a 1984 video game, and 2005 feature film.",
                ISBN = "9780345391803",
                Author = "Douglas Adams",
                ListPrice = 12.99,
                Price = 11.99,
                Price50 = 10.99,
                Price100 = 9.99,
                CategoryId = 1,
                ImageUrl="https://images-na.ssl-images-amazon.com/images/I/51Zymoq7UnL._AC_SY400_.jpg"
            },
            new Product
            {
                ProductId = 2,
                Title = "The Hobbit",
                Description = "The Hobbit, or There and Back Again is a children's fantasy novel by English author J. R. R. Tolkien. It was published on 21 September 1937 to wide critical acclaim, being nominated for the Carnegie Medal and awarded a prize from the New York Herald Tribune for best juvenile fiction.",
                ISBN = "9780345339683",
                Author = "J.R.R. Tolkien",
                ListPrice = 14.99,
                Price = 13.99,
                Price50 = 12.99,
                Price100 = 11.99,
                CategoryId = 2,
                ImageUrl="https://images-na.ssl-images-amazon.com/images/I/51Zymoq7UnL._AC_SY400_.jpg"
            });

            modelBuilder.Entity<Company>().HasData(new Company
            {
                CompanyId = 1,
                Name = "Company1",
                Address = "123 Main St",
                City = "New York",
                State = "NY",
                PostalCode = "10000",
                PhoneNumber = "123-456-7890"
            },
            new Company
            {
                CompanyId = 2,
                Name = "Company2",
                Address = "123 Main St",
                City = "New York",
                State = "NY",
                PostalCode = "10000",
                PhoneNumber = "123-456-7890"
            });

        }
    }


}