using Shared.DTOs;
using Data.Entities;

namespace Services.Interfaces
{
    public interface IUserService
    {
        Task<IEnumerable<UserDto>> GetAllUsersAsync();
        Task<UserDto?> GetUserByIdAsync(Guid id);
        Task<UserDto> CreateUserAsync(UserDto userDto);
    }
}
