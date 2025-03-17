using Shared.Enums;

namespace Api.DTOs
{
    public class UserResponse
    {
        public string Email { get; set; }
        public string Username { get; set; }
        public RoleType Role { get; set; }
    }
}
