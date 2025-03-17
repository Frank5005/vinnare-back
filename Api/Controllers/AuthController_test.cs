using Api.Controllers;
using Api.DTOs;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Services.Interfaces;
using Shared.DTOs;
using Shared.Exceptions;
using Xunit;

public class AuthController_test
{
    private readonly Mock<ITokenService> _mockTokenService;
    private readonly Mock<IUserService> _mockUserService;
    private readonly Mock<IPasswordHasher> _mockPasswordHasher;
    private readonly AuthController _authController;

    public AuthController_test()
    {
        _mockTokenService = new Mock<ITokenService>();
        _mockUserService = new Mock<IUserService>();
        _mockPasswordHasher = new Mock<IPasswordHasher>();

        _authController = new AuthController(_mockTokenService.Object, _mockUserService.Object, _mockPasswordHasher.Object);
    }

    [Fact]
    public async Task Login_ShouldReturnOk_WhenCredentialsAreValid()
    {
        // Arrange
        var request = new LoginRequest { Username = "testuser", Password = "validpassword" };
        var user = new UserDto { Username = "testuser", Password = "hashedpassword", Role = Shared.Enums.RoleType.Admin };

        _mockUserService.Setup(s => s.GetUserByUsername(request.Username)).ReturnsAsync(user);
        _mockPasswordHasher.Setup(h => h.VerifyPassword(request.Password, user.Password)).Returns(true);
        _mockTokenService.Setup(t => t.GenerateToken(request.Username, "Admin")).Returns("valid_token");

        // Act
        var result = await _authController.Login(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var tokenResponse = Assert.IsType<TokenResponse>(okResult.Value);
        Assert.Equal("valid_token", tokenResponse.AccessToken);
    }

    [Fact]
    public async Task Login_ShouldThrowNotFoundException_WhenUserDoesNotExist()
    {
        // Arrange
        var request = new LoginRequest { Username = "nonexistentuser", Password = "password" };
        _mockUserService.Setup(s => s.GetUserByUsername(request.Username)).ReturnsAsync((UserDto)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _authController.Login(request));
    }

    [Fact]
    public async Task Login_ShouldThrowUnauthorizedException_WhenPasswordIsIncorrect()
    {
        // Arrange
        var request = new LoginRequest { Username = "testuser", Password = "wrongpassword" };
        var user = new UserDto { Username = "testuser", Password = "hashedpassword", Role = Shared.Enums.RoleType.Admin };

        _mockUserService.Setup(s => s.GetUserByUsername(request.Username)).ReturnsAsync(user);
        _mockPasswordHasher.Setup(h => h.VerifyPassword(request.Password, user.Password)).Returns(false);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedException>(() => _authController.Login(request));
    }
}
