using Shared.DTOs;

namespace Services.Interfaces
{
    public interface IUserService
    {
        Task<IEnumerable<UserDto>> GetAllUsersAsync();
        Task<UserDto?> GetUserByIdAsync(Guid id);
        Task<UserDto?> GetUserByUsername(string username);
        Task<UserDto> CreateUserAsync(UserDto userDto);

        Task<UserDto?> UpdateUserAsync(Guid id, UserDto userDto);

        Task<UserDto?> DeleteUserAsync(Guid id);
    }
}
