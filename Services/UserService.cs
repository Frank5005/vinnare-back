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
        private readonly IPasswordHasher _passwordHasher;


        public UserService(VinnareDbContext context, IPasswordHasher passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
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
            string hashedPassword = _passwordHasher.HashPassword(userDto.Password);

            var user = new User
            {
                Email = userDto.Email,
                Username = userDto.Username,
                Password = hashedPassword,
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

        public async Task<UserDto?> UpdateUserAsync(Guid id, UserDto userDto)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return null;

            user.Email = userDto.Email;
            user.Username = userDto.Username;
            user.Role = userDto.Role;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                Username = user.Username,
                Role = user.Role
            };
        }

        public async Task<UserDto?> DeleteUserAsync(Guid id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return null;

            _context.Users.Remove(user);
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
