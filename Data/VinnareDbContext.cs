using Data.Entities;
using Microsoft.EntityFrameworkCore;


namespace Data
{
    public class VinnareDbContext : DbContext
    {
        public VinnareDbContext(DbContextOptions<VinnareDbContext> options)
            : base(options) { }

        // Define your DbSets here
        public DbSet<User> Users { get; set; }
        public DbSet<Product> Products { get; set; }

        public DbSet<Category> Categories { get; set; }

        public DbSet<Review> Reviews { get; set; }

        public DbSet<WishList> WishLists { get; set; }

        public DbSet<Cart> Carts { get; set; }

        public DbSet<Coupon> Coupons { get; set; }

        public DbSet<Purchase> Purchases { get; set; }

        public DbSet<Job> Jobs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Unique Constraints
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            // Relationship

            modelBuilder.Entity<Category>()
                .Property(c => c.Id)
                .HasColumnType("integer")
                .UseIdentityColumn();

            modelBuilder.Entity<Category>()
                .Property(c => c.Approved)
                .HasDefaultValue(false);

            modelBuilder.Entity<Product>()
                .Property(p => p.Id)
                .HasColumnType("integer")
                .UseIdentityColumn();

            modelBuilder.Entity<Product>()
                .HasOne(p => p.Owner)
                .WithMany(u => u.Products)
                .HasForeignKey(p => p.OwnerId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Product>()
                .Property(c => c.Approved)
                .HasDefaultValue(false);

            modelBuilder.Entity<Review>(entity =>
            {

                entity.Property(r => r.Id)
                    .HasColumnType("integer")
                    .UseIdentityColumn();

                entity.HasOne(r => r.User)
                    .WithMany(u => u.Reviews)
                    .HasForeignKey(r => r.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(r => r.Product)
                    .WithMany(p => p.Reviews)
                    .HasForeignKey(r => r.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<WishList>()
                .Property(w => w.Id)
                .HasColumnType("integer")
                .UseIdentityColumn();

            modelBuilder.Entity<WishList>()
                .HasOne(w => w.User)
                .WithMany(u => u.WishLists)
                .HasForeignKey(w => w.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<WishList>()
                .HasOne(w => w.Product)
                .WithMany(p => p.WishLists)
                .HasForeignKey(w => w.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Cart>()
                .Property(c => c.Id)
                .HasColumnType("integer")
                .UseIdentityColumn();

            modelBuilder.Entity<Cart>()
                .HasOne(c => c.User)
                .WithMany(u => u.Carts)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Cart>()
                .HasOne(c => c.Product)
                .WithMany(p => p.Carts)
                .HasForeignKey(c => c.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            //Coupons
            modelBuilder.Entity<Coupon>()
                .Property(c => c.Id)
                .HasColumnType("integer")
                .UseIdentityColumn();

            //Purchases
            modelBuilder.Entity<Purchase>()
                .Property(p => p.Id)
                .HasColumnType("integer")
                .UseIdentityColumn();

            modelBuilder.Entity<Purchase>()
                .HasOne(p => p.User)
                .WithMany(u => u.Purchases)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            //Jobs
            modelBuilder.Entity<Job>(entity =>
            {
                entity.Property(j => j.Id)
                .HasColumnType("integer")
                .UseIdentityColumn();

                entity.HasOne(j => j.User)
                .WithMany(u => u.Jobs)
                .HasForeignKey(j => j.CreatorId)
                .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(j => j.Product)
                .WithMany(p => p.Jobs)
                .HasForeignKey(j => j.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(j => j.Category)
                .WithMany(c => c.Jobs)
                .HasForeignKey(j => j.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);
            });

        }
    }

}
