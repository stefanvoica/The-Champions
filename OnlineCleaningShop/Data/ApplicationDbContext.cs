using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OnlineCleaningShop.Models;
using static OnlineCleaningShop.Models.ProductBookmarks;

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
        public DbSet<Category> Categories { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Bookmark> Bookmarks { get; set; }
        public DbSet<ProductBookmark> ProductBookmarks { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // definirea relatiei many-to-many dintre Product si Bookmark

            base.OnModelCreating(modelBuilder);

            // definire primary key compus
            modelBuilder.Entity<ProductBookmark>()
                .HasKey(ab => new { ab.Id, ab.ProductId, ab.BookmarkId });

            // definire relatii cu modelele Bookmark si Product (FK)

            modelBuilder.Entity<ProductBookmark>()
                .HasOne(ab => ab.Product)
                .WithMany(ab => ab.ProductBookmarks)
                .HasForeignKey(ab => ab.ProductId);

            modelBuilder.Entity<ProductBookmark>()
                .HasOne(ab => ab.Bookmark)
                .WithMany(ab => ab.ProductBookmarks)
                .HasForeignKey(ab => ab.BookmarkId);
        }
    }
}
