using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OnlineCleaningShop.Models;

namespace OnlineCleaningShop.Data
{
    //PASUL 3: useri si roluri
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
           : base(options)
        {
        }
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductRequest> ProductRequests { get; set; }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<CodPromotional> CoduriPromotionale { get; set; }
        public DbSet<NewsletterSubscriber> NewsletterSubscribers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // definirea relatiei many-to-many dintre PRODUCT si ORDER

            base.OnModelCreating(modelBuilder);

            // definire primary key compus
            modelBuilder.Entity<OrderDetail>()
                .HasKey(ab => new { ab.Id, ab.ProductId, ab.OrderId });


            // definirea relațiilor dintre OrderDetail, Product și Order (FK)

            modelBuilder.Entity<OrderDetail>()
                .HasOne(ab => ab.Product)
                .WithMany(ab => ab.OrderDetails)
                .HasForeignKey(ab => ab.ProductId);

            modelBuilder.Entity<OrderDetail>()
                .HasOne(ab => ab.Order)
                .WithMany(ab => ab.OrderDetails)
                .HasForeignKey(ab => ab.OrderId);
        }
    }
}
