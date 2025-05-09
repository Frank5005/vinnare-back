using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
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
        public async Task<IEnumerable<UserDtoString>> GetAllUsersAsync()
        {
            return await _context.Users
                .Select(user => new UserDtoString
                {
                    Name = user.Name,
                    Email = user.Email,
                    Username = user.Username,
                    Password = user.Password,
                    Role = user.Role.ToString(),
                    Address = user.Address,
                    SecurityAnswer = user.SecurityAnswer,
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

        public async Task<string> GetUsernameById(Guid id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return null;

            return user.Username;
        }

        public async Task<UserDto?> GetUserByUsername(string username)
        {
            var userQuery = from n in _context.Users
                            where n.Username == username
                            select n;

            var user = await userQuery.FirstOrDefaultAsync();

            if (user == null) return null;

            return new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                Username = user.Username,
                Password = user.Password,
                Role = user.Role
            };
        }

        public async Task<Guid?> GetIdByUsername(string username)
        {

            var userQuery = from n in _context.Users
                            where n.Username == username
                            select n.Id;

            var id = await userQuery.FirstOrDefaultAsync();

            return id == Guid.Empty ? null : id;
        }

        

        public async Task<Guid> GetUserIdFromToken(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);

            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out Guid userId))
            {
                throw new Exception("User ID not found or invalid in token");
            }

            return userId;
        }

        public async Task<UserDto> CreateUserAsync(UserDto userDto)
        {
            // Check if user already exists
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == userDto.Username || u.Email == userDto.Email);
            if (existingUser != null)
            {
                if (existingUser.Username == userDto.Username)
                    throw new Exception("Username already exists");
                if (existingUser.Email == userDto.Email)
                    throw new Exception("Email already exists");
            }

            string hashedPassword = _passwordHasher.HashPassword(userDto.Password);

            var user = new User
            {
                Email = userDto.Email,
                Username = userDto.Username,
                Name = userDto.Name,
                Password = hashedPassword,
                Role = userDto.Role,
                Date = DateTime.UtcNow,
                Address = userDto.Address,
                SecurityQuestion = userDto.SecurityQuestion,
                SecurityAnswer = userDto.SecurityAnswer
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

            // Update only non-null or non-empty fields
            if (!string.IsNullOrWhiteSpace(userDto.Username))
                user.Username = userDto.Username; // Username is mandatory, already validated in controller

            if (!string.IsNullOrWhiteSpace(userDto.Email))
                user.Email = userDto.Email;

            if (!string.IsNullOrWhiteSpace(userDto.Password))
                user.Password = _passwordHasher.HashPassword(userDto.Password); // Hash password before saving

            if (!string.IsNullOrWhiteSpace(userDto.Name))
                user.Name = userDto.Name; // Optional field, update if provided

            // Update in DB
            // Update only non-null or non-empty fields
            if (!string.IsNullOrWhiteSpace(userDto.Username))
                user.Username = userDto.Username; // Username is mandatory, already validated in controller

            if (!string.IsNullOrWhiteSpace(userDto.Email))
                user.Email = userDto.Email;

            if (!string.IsNullOrWhiteSpace(userDto.Password))
                user.Password = _passwordHasher.HashPassword(userDto.Password); // Hash password before saving

            if (!string.IsNullOrWhiteSpace(userDto.Name))
                user.Name = userDto.Name; // Optional field, update if provided

            // Update in DB
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                Username = user.Username,
                Name = user.Name,
                Role = user.Role
            };
        }
        public async Task<List<UserDto>> DeleteUsersAsync(List<string> usernames)
        {
            var users = await _context.Users.Where(u => usernames.Contains(u.Username)).ToListAsync();

            if (!users.Any()) return new List<UserDto>(); // No users found

            _context.Users.RemoveRange(users);
            _context.Users.RemoveRange(users);
            await _context.SaveChangesAsync();

            return users.Select(user => new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                Username = user.Username,
                Role = user.Role
            }).ToList();
        }


    }
}
