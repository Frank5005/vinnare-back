using System.Security.Claims;
using Api.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;
using Shared.DTOs;
using Shared.Exceptions;
namespace Api.Controllers
{
    [Route("api/user")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        // GET: api/users
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }

        // UPDATE: api/user
        [Authorize(Roles = "Admin")]
        [HttpPut]
        public async Task<IActionResult> UpdateUser([FromBody] UpdateUserRequest UpdateRequest)
        {
            if (UpdateRequest == null || string.IsNullOrWhiteSpace(UpdateRequest.Username))
                return BadRequest("User data is required.");

            if (UpdateRequest.Email != null || !string.IsNullOrWhiteSpace(UpdateRequest.Username))
            {
                UpdateRequest.Validate();
            }
            var Id = await _userService.GetIdByUsername(UpdateRequest.Username) ?? throw new NotFoundException("user does not exists");
            var userDto = new UserDto
            {
                Id = Id,
                Username = UpdateRequest.Username,
                Email = UpdateRequest.Email,
                Password = UpdateRequest.Password,
                Name = UpdateRequest.Name,
                Address = UpdateRequest.Address,
                //SecurityQuestion = UpdateRequest.GetSecurityQuestionType(),
                SecurityAnswer = UpdateRequest.SecurityAnswer


            };
            var updatedUser = await _userService.UpdateUserAsync(Id, userDto);
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
    }
}
