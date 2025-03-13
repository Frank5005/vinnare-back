using System.ComponentModel.DataAnnotations;
using Shared.Enums;

namespace Data.Entities
{
    public class User
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

        [Required]
        public RoleType Role { get; set; }

        // Navigation Property
        public ICollection<Product> Products { get; set; }
        public ICollection<Review> Reviews { get; set; }
        public ICollection<WishList> WishLists { get; set; } //= new List<WishList>();
        //public ICollection<Purchase> Purchases { get; set; } //= new List<Purchase>();
        //public ICollection<Job> Jobs { get; set; } //= new List<Job>();
        public ICollection<Cart> Carts { get; set; } //= new List<Cart>();
    }
}
