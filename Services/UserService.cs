using Data;
using Data.Entities;
using Microsoft.EntityFrameworkCore;
using Services.Interfaces;
using Shared.DTOs;

namespace Services
{
    public class UserService : IUserService
    {
        private readonly VinnareDbContext _context;

        public UserService(VinnareDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
        {
            return await _context.Users
                .Select(user => new UserDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    Username = user.Username,
                    Role = user.Role
                })
                .ToListAsync();
        }

        public async Task<UserDto?> GetUserByIdAsync(Guid id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return null;

            return new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                Username = user.Username,
                Role = user.Role
            };
        }

        public async Task<UserDto> CreateUserAsync(UserDto userDto)
        {
            var user = new User
            {
                Email = userDto.Email,
                Username = userDto.Username,
                Password = userDto.Password, // In real-world, hash this!
                Role = userDto.Role
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                Username = user.Username,
                Role = user.Role
            };
        }
    }
}
