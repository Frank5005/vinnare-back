using Api.Utils;
using Shared.Exceptions;

namespace Api.DTOs
{
    public class BaseUpdateUserRequest
    {
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public void Validate()
        {
            if (!EmailValidator.IsValidEmail(Email))
            {
                throw new BadRequestException("Invalid email format.");
            }
        }
    }
    public class UpdateUserRequest : BaseUpdateUserRequest
    {
        public string Username { get; set; } = "";
    }

    public class UpdateShoppperRequest : BaseUpdateUserRequest
    {
    }

    public class DeleteUserRequest
    {
        public List<string> Users { get; set; } = new List<string>();
    }
}
