using System.Text.Json.Serialization;
using Shared.Enums;

namespace Shared.DTOs
{
    public class UserBase
    {
        public string? Name { get; set; }
        public string Email { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Address { get; set; }
        //public SecurityQuestionType SecurityQuestion { get; set; }
        public string SecurityAnswer { get; set; }
    }

    public class VerifyUserRequest
    {
        public string Email { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public SecurityQuestionType SecurityQuestion { get; set; }
        public string SecurityAnswer { get; set; }
    }

    public class UserDto : UserBase
    {
        public Guid Id { get; set; }
        public RoleType Role { get; set; }
        public SecurityQuestionType SecurityQuestion { get; set; }
    }

    public class UserDtoString : UserBase
    {
        public String Role { get; set; }

    }

}
