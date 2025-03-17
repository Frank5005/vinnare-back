using Api.Utils;
using Shared.Enums;
using Shared.Exceptions;

namespace Api.DTOs
{
    public class LoginRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class LoginResponse
    {
        public string Token { get; set; }
        public string Email { get; set; }
        public string Username { get; set; }
    }
    public abstract class UserRequestBase
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public void Validate()
        {
            if (!EmailValidator.IsValidEmail(Email))
            {
                throw new BadRequestException("Invalid email format.");
            }
        }
    }
    public class UserCreateShopperRequest : UserRequestBase
    {

    }
    public class UserCreateRequest : UserRequestBase
    {
        public string Role { get; set; }

        public RoleType GetRoleType()
        {
            if (Enum.TryParse<RoleType>(Role, true, out var parsedRole))
            {
                return parsedRole;
            }
            throw new BadRequestException("Invalid role type.");
        }
    }
    public class UserResponse
    {
        public Guid Id { get; set; }
        public string Email { get; set; }
        public string Username { get; set; }
    }


}
