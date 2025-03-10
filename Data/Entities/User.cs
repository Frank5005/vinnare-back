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
        public List<Product> Products { get; set; } = new();
    }
}
