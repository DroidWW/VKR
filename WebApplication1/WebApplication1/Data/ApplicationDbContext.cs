using Microsoft.EntityFrameworkCore;
using WebApplication1.Data.Models;

namespace WebApplication1.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) 
            : base(options) 
        {
        }

        public DbSet<Users> Users { get; set; }
        public DbSet<Reports> Reports { get; set; }
        public DbSet<Products> Products { get; set; }
        public DbSet<RepairOrders> RepairOrders { get; set; }
        public DbSet<RepairOrderImages> RepairOrderImages { get; set; }
        public DbSet<ProfileUserImages> ProfileUserImages { get; set; }
        public DbSet<Messages> Messages { get; set; }
        public DbSet<ReportImages> ReportImages { get; set; }
        public DbSet<ProductImages> ProductImages { get; set; }
        public DbSet<LikesDislikes> LikesDislikes { get; set; }
        public DbSet<ReportsComments> ReportsComments { get; set; }
        public DbSet<Services> Services { get; set; }
        public DbSet<ShopOrders> ShopOrders { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Users>().HasKey(u => u.UserID);
            modelBuilder.Entity<Reports>().HasKey(u => u.ReportID);
            modelBuilder.Entity<Products>().HasKey(u => u.ProductID);
            modelBuilder.Entity<RepairOrders>().HasKey(u => u.OrderID);
            modelBuilder.Entity<RepairOrderImages>().HasKey(u => u.ImageID);
            modelBuilder.Entity<ProfileUserImages>().HasKey(u => u.ImageID);
            modelBuilder.Entity<Messages>().HasKey(u => u.MessageID);
            modelBuilder.Entity<ReportImages>().HasKey(u => u.ImageID);
            modelBuilder.Entity<ProductImages>().HasKey(u => u.ImageID);
            modelBuilder.Entity<ReportsComments>().HasKey(u => u.CommentID);
            modelBuilder.Entity<LikesDislikes>().HasKey(u => u.ID);
            modelBuilder.Entity<Services>().HasKey(u => u.ServiceID);
            modelBuilder.Entity<ShopOrders>().HasKey(u => u.ShopOrderID);


            base.OnModelCreating(modelBuilder);
        }
    }
}
