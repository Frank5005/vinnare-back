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

        //// GET: api/user/{id}
        //[HttpGet("{id:guid}")]
        //public async Task<IActionResult> GetUserById(Guid id)
        //{
        //    var user = await _userService.GetUserByIdAsync(id);
        //    if (user == null) return NotFound();
        //    return Ok(user);
        //}

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

            return NoContent(); // 204 No Content, indicating success with no response body
        }
    }
}
