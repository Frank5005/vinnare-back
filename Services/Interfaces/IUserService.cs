using Shared.DTOs;

namespace Services.Interfaces
{
    public interface IUserService
    {
        Task<IEnumerable<UserDtoString>> GetAllUsersAsync();
        Task<UserDto?> GetUserByIdAsync(Guid id);
        Task<UserDto?> GetUserByUsername(string username);
        Task<Guid?> GetIdByUsername(string username);
        Task<Guid?> GetIdByEmail(string email);
        Task<string> GetUsernameById(Guid id);
        Task<Guid> GetUserIdFromToken(string token);
        Task<UserDto> CreateUserAsync(UserDto userDto);

        Task<UserDto?> UpdateUserAsync(Guid id, UserDto userDto);
        Task<List<UserDto>> DeleteUsersAsync(List<string> usernames);
    }
}
