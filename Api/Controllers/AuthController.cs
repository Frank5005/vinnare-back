using Api.DTOs;
using Data.Entities;
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
            var user = await _userService.GetUserByEmail(request.Email);
            if (user == null)
            {
                throw new NotFoundException("Username not found");
            }
            var passed = _passwordHasher.VerifyPassword(request.Password, user.Password);
            if (passed == false)
            {
                throw new UnauthorizedException("Wrong password");
            }
            ;
            string token = _tokenService.GenerateToken(request.Email, user.Role.ToString());
            //string token = _tokenService.GenerateToken(request.Username, "Admin"); //TODO: implement actual user.Role

            return Ok(new LoginResponse { Token = token, Email = user.Email, Username = user.Username });
        }

        // POST: api/verify
        [AllowAnonymous]
        [HttpPost("verify")]
        public async Task<IActionResult> Verify([FromBody] verifyRequest request)
        {
            var user = await _userService.GetUserByEmail(request.Email);

            if (user == null)
            {
                throw new NotFoundException("User not found");
            }

            var parsedQuestion = request.GetSecurityQuestionType();

            if (user.SecurityQuestion != parsedQuestion ||
                !string.Equals(user.SecurityAnswer, request.SecurityAnswer, StringComparison.OrdinalIgnoreCase))
            {
                return Unauthorized(new { message = "Security question or answer is incorrect" });
            }

            return Ok(new { message = "User is valid" });
        }

        // POST: api/reset-password
        [AllowAnonymous]
        [HttpPut("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            var user = await _userService.GetUserByEmail(request.Email);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            user.Password = _passwordHasher.HashPassword(request.NewPassword);

            var userDto = new UserDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Username = user.Username,
                Password = user.Password,
                Address = user.Address,
                Role = user.Role,
                SecurityQuestion = user.SecurityQuestion,
                SecurityAnswer = user.SecurityAnswer
            };

            await _userService.UpdateUserPsw(userDto);

            return Ok(new { message = "Password updated successfully" });
        }


        // GET: api/security-questions
        [HttpGet("security-questions")]
        public IActionResult GetSecurityQuestions()
        {
            var questions = Enum.GetValues(typeof(SecurityQuestionType))
                .Cast<SecurityQuestionType>()
                .Select(q => new
                {
                    value = q,
                    label = string.Concat(
                        System.Text.RegularExpressions.Regex.Replace(q.ToString(), "([a-z])([A-Z])", "$1 $2")
                    )
                });

            return Ok(questions);
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
                Name = userRequest.Name,
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
                Name = userRequest.Name,
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
            throw new NullReferenceException("A wild exception appeared!. It used NullReferenceException. It's super effective... ");
        }
        /*
        [HttpGet("Dashboard")]
        public void test()
        {

        }
        */
    }
}
