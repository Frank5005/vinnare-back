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
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Owner)
                .WithMany(u => u.Products)
                .HasForeignKey(p => p.OwnerId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }

}
