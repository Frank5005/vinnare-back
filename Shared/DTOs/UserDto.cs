using Shared.Enums;

namespace Shared.DTOs
{
    public class UserDto
    {
        public Guid Id { get; set; }
        public string Email { get; set; }
        public string Username { get; set; }
        public string Password { get; set; } // Consider hashing this
        public RoleType Role { get; set; }
    }
}
