using Api.Utils;
using Shared.DTOs;
using Shared.Enums;
using Shared.Exceptions;

namespace Api.DTOs
{
    public class LoginRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class verifyRequest
    {
        public string Email { get; set; }
        public string SecurityQuestion { get; set; }
        public string SecurityAnswer { get; set; }
    }

    public class ResetPasswordRequest
{
    public string Email { get; set; }
    public string NewPassword { get; set; }
}


    public class LoginResponse
    {
        public string Token { get; set; }
        public string Email { get; set; }
        public string Username { get; set; }
    }
    public abstract class UserRequestBase : UserBase
    {
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
        public string SecurityQuestion { get; set; }
        public SecurityQuestionType GetSecurityQuestionType()
        {
            if (Enum.TryParse<SecurityQuestionType>(SecurityQuestion, true, out var parsedRole))
            {
                return parsedRole;
            }
            throw new BadRequestException("Invalid Security Question type.");
        }
    }
    public class UserCreateRequest : UserRequestBase
    {
        public string Role { get; set; }
        public string SecurityQuestion { get; set; }

        public RoleType GetRoleType()
        {
            if (Enum.TryParse<RoleType>(Role, true, out var parsedRole))
            {
                return parsedRole;
            }
            throw new BadRequestException("Invalid role type.");
        }
        public SecurityQuestionType GetSecurityQuestionType()
        {
            if (Enum.TryParse<SecurityQuestionType>(SecurityQuestion, true, out var parsedRole))
            {
                return parsedRole;
            }
            throw new BadRequestException("Invalid Security Question type.");
        }
    }
    public class UserResponse
    {
        public Guid Id { get; set; }
        public string Email { get; set; }
        public string Username { get; set; }
    }


}
