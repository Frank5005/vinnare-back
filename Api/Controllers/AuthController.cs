using Api.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;
using Shared.DTOs;
using Shared.Enums;
using Shared.Exceptions;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/")]
    public class AuthController : ControllerBase
    {
        private readonly ITokenService _tokenService;
        private readonly IUserService _userService;
        private readonly IPasswordHasher _passwordHasher;
        public AuthController(ITokenService tokenService, IUserService userService, IPasswordHasher passwordHasher)
        {
            _tokenService = tokenService;
            _userService = userService;
            _passwordHasher = passwordHasher;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var user = await _userService.GetUserByUsername(request.Username);
            if (user == null)
            {
                throw new NotFoundException("Username not found");
            }
            var passed = _passwordHasher.VerifyPassword(request.Password, user.Password);
            if (passed == false)
            {
                throw new UnauthorizedException("Wrong password");
            };
            string token = _tokenService.GenerateToken(request.Username, user.Role.ToString());
            //string token = _tokenService.GenerateToken(request.Username, "Admin"); //TODO: implement actual user.Role

            return Ok(new LoginResponse { Token = token, Email = user.Email, Username = user.Username });
        }

        // POST: api/admin/auth
        [Authorize(Roles = "Admin")]
        [HttpPost("admin/auth")]
        public async Task<IActionResult> CreateUser([FromBody] UserCreateRequest userRequest)
        {
            userRequest.Validate();
            if (userRequest.GetRoleType() == RoleType.Admin)
            {
                throw new BadRequestException("sadly admins can't be created");
            }

            var userDto = new UserDto
            {
                Id = Guid.NewGuid(),
                Email = userRequest.Email,
                Username = userRequest.Username,
                Password = userRequest.Password,
                Address = userRequest.Address,
                Role = userRequest.GetRoleType(),
                SecurityQuestion = userRequest.GetSecurityQuestionType(),
                SecurityAnswer = userRequest.SecurityAnswer
            };

            var createdUser = await _userService.CreateUserAsync(userDto);

            var responseUser = new UserResponse
            {
                Id = createdUser.Id,
                Email = createdUser.Email,
                Username = createdUser.Username,
            };
            return Created("", responseUser);
        }



        // POST: api/auth
        [HttpPost("auth")]
        public async Task<IActionResult> CreateShopper([FromBody] UserCreateShopperRequest userRequest)
        {
            userRequest.Validate();

            var userDto = new UserDto
            {
                Id = Guid.NewGuid(),
                Email = userRequest.Email,
                Username = userRequest.Username,
                Password = userRequest.Password,
                Address = userRequest.Address,
                Role = RoleType.Shopper,
                SecurityQuestion = userRequest.GetSecurityQuestionType(),
                SecurityAnswer = userRequest.SecurityAnswer
            };

            var createdUser = await _userService.CreateUserAsync(userDto);

            var responseUser = new UserResponse
            {
                Id = createdUser.Id,
                Email = createdUser.Email,
                Username = createdUser.Username,
            };
            return Created("", responseUser);
        }


        [HttpGet("fail")]
        public void fail()
        {
            throw new NullReferenceException("A wild exception appeared!. It used NullReferenceException. It�s super effective... ");
        }

        [HttpGet("Dashboard")]
        public void test()
        {

        }
    }
}
