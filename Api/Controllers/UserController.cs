using System.Security.Claims;
using Api.DTOs;
using Api.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;
using Shared.DTOs;
using Shared.Enums;
using Shared.Exceptions;
namespace Api.Controllers
{
    [Route("api/user")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ITokenService _tokenService;

        public UserController(IUserService userService, ITokenService tokenService)
        {
            _userService = userService;
            _tokenService = tokenService;
        }

        // GET: api/user/list
        [Authorize(Roles = "Admin")]
        [HttpGet("list")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }

        // GET: api/user/name
        [HttpGet("name")]
        public async Task<IActionResult> GetName(Guid ownerId)
        {
            var username = await _userService.GetNameById(ownerId);
            if (username == null)
            {
                throw new NotFoundException("Username not found");
            }
            return Ok(username);
        }

        // POST: api/user/verify
        [HttpPost("verify")]
        public async Task<IActionResult> VerifyUser([FromBody] VerifyUserRequest verifyRequest)
        {
            var user = await _userService.GetUserByEmail(verifyRequest.Email);
            if (user == null)
            {
                throw new NotFoundException("Username not found");
            }
            if (user.SecurityQuestion != verifyRequest.SecurityQuestion)
                return BadRequest("Incorrect security question");

            if ((user.SecurityAnswer?.Trim().ToLower()) != (verifyRequest.SecurityAnswer?.Trim().ToLower()))
            {
                return BadRequest("Incorrect answer");
            }

            if (user.Role == RoleType.Admin || user.Role == RoleType.Seller)
                return BadRequest("This password cannot be reset because the user is an admin or seller");

            var token = _tokenService.GenerateToken(user.Email, user.Role.ToString());
            return Ok(new { token });
        }

        // UPDATE: api/user
        //[Authorize(Roles = "Admin")]
        [HttpPut]
        public async Task<IActionResult> UpdateUser([FromBody] UpdateUserRequest UpdateRequest)
        {
            if (UpdateRequest == null)
                return BadRequest("User data is required.");
            // Getting the token from the request header
            var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var email = _tokenService.GetEmailFromToken(token);
            if (string.IsNullOrEmpty(email))
                return Unauthorized("Invalid or expired token.");

            // Validating the email
            if (!EmailValidator.IsValidEmail(email))
                return BadRequest("Invalid email from token.");

            // Getting the user ID from the email
            var id = await _userService.GetIdByEmail(email) ?? throw new NotFoundException("User does not exist");
            var userDto = new UserDto
            {
                //Id = UpdaId,
                Username = UpdateRequest.Username,
                Email = UpdateRequest.Email,
                Password = UpdateRequest.Password,
                Name = UpdateRequest.Name,
                Address = UpdateRequest.Address,
                //SecurityQuestion = UpdateRequest.GetSecurityQuestionType(),
                SecurityAnswer = UpdateRequest.SecurityAnswer


            };
            var updatedUser = await _userService.UpdateUserAsync(id, userDto);
            if (updatedUser == null) return NotFound();
            return Ok(new DefaultResponse
            {
                message = $"User {UpdateRequest.Username} has been updated successfully"
            });

        }

        // DELETE: api/user
        [Authorize(Roles = "Admin")]

        [HttpDelete]
        public async Task<IActionResult> DeleteUsers([FromBody] DeleteUserRequest deleteRequest)
        {
            if (deleteRequest == null || deleteRequest.Users == null || !deleteRequest.Users.Any())
                return BadRequest("At least one username must be provided.");

            var deletedUsers = await _userService.DeleteUsersAsync(deleteRequest.Users);

            if (!deletedUsers.Any())
                return NotFound("No matching users found to delete.");

            return NoContent();
        }

        // UPDATE: api/user/shopper
        [Authorize]
        [HttpPut("shopper")]
        public async Task<IActionResult> UpdateShopper([FromBody] UpdateShoppperRequest updateRequest)
        {

            var tokenUsername = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(tokenUsername))
            {
                throw new UnauthorizedException("Token does not contain a username.");
            }

            if (updateRequest == null)// || string.IsNullOrWhiteSpace(updateRequest.Username))
                throw new BadRequestException("User data is required.");

            if (updateRequest.Email != null || !string.IsNullOrWhiteSpace(tokenUsername))
            {
                updateRequest.Validate();
            }

            var Id = await _userService.GetIdByUsername(tokenUsername) ?? throw new NotFoundException("user does not exists");
            var userDto = new UserDto
            {
                Id = Id,
                Username = tokenUsername,
                Email = updateRequest.Email,
                Password = updateRequest.Password,
                Name = updateRequest.Name,
                Address = updateRequest.Address,
                //SecurityQuestion = updateRequest.GetSecurityQuestionType(),
                SecurityAnswer = updateRequest.SecurityAnswer

            };
            var updatedUser = await _userService.UpdateUserAsync(Id, userDto);
            if (updatedUser == null) return NotFound();
            return Ok(new DefaultResponse
            {
                message = $"User {tokenUsername} has been updated successfully"
            });

        }

        // DELETE: api/user/shopper
        [Authorize]
        [HttpDelete("shopper")]
        public async Task<IActionResult> DeleteShopper()
        {
            var tokenUsername = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(tokenUsername))
            {
                throw new UnauthorizedException("Token does not contain a username.");
            }

            var deletedUsers = await _userService.DeleteUsersAsync([tokenUsername]);

            if (!deletedUsers.Any())
                throw new NotFoundException("How did we get here? you are a user but not really");

            return NoContent();
        }

        // GET: api/user/id/{username}
        [HttpGet("id/{username}")]
        public async Task<IActionResult> GetUserIdByUsername(string username)
        {
            if (string.IsNullOrEmpty(username))
                return BadRequest("Username is required.");

            var userId = await _userService.GetIdByUsername(username);
            if (userId == null)
                return NotFound($"No user found with username: {username}");

            return Ok(new { Id = userId });
        }
    }
}
