using Api.DTOs;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;
using Shared.Exceptions;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/auth")]
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
            //string token = _tokenService.GenerateToken(request.Username, user.Role);
            string token = _tokenService.GenerateToken(request.Username, "Admin"); //TODO: implement actual user.Role

            return Ok(new TokenResponse { AccessToken = token });
        }
    }
}
