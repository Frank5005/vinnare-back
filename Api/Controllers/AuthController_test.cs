using Api.Controllers;
using Api.DTOs;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Services.Interfaces;
using Shared.DTOs;
using Shared.Enums;
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

        _authController = new AuthController(
            _mockTokenService.Object,
            _mockUserService.Object,
            _mockPasswordHasher.Object
        );
    }

    // ===== LOGIN TESTS =====

    [Fact]
    public async Task Login_ShouldReturnOk_WhenCredentialsAreValid()
    {
        // Arrange
        var request = new LoginRequest { Username = "testuser", Password = "validpassword" };
        var user = new UserDto { Username = "testuser", Password = "hashedpassword", Role = RoleType.Admin };

        _mockUserService.Setup(s => s.GetUserByUsername(request.Username)).ReturnsAsync(user);
        _mockPasswordHasher.Setup(h => h.VerifyPassword(request.Password, user.Password)).Returns(true);
        _mockTokenService.Setup(t => t.GenerateToken(request.Username, "Admin")).Returns("valid_token");

        // Act
        var result = await _authController.Login(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var tokenResponse = Assert.IsType<LoginResponse>(okResult.Value);
        Assert.Equal("valid_token", tokenResponse.Token);
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
        var user = new UserDto { Username = "testuser", Password = "hashedpassword", Role = RoleType.Admin };

        _mockUserService.Setup(s => s.GetUserByUsername(request.Username)).ReturnsAsync(user);
        _mockPasswordHasher.Setup(h => h.VerifyPassword(request.Password, user.Password)).Returns(false);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedException>(() => _authController.Login(request));
    }

    // ===== CREATE USER TESTS (ADMIN) =====

    [Fact]
    public async Task CreateUser_ShouldReturnCreated_WhenValidRequest()
    {
        // Arrange
        var request = new UserCreateRequest
        {
            Email = "admin@example.com",
            Username = "adminUser",
            Password = "securepassword",
            Role = "Seller",
            SecurityQuestion = "WhatIsYourFavoriteColor",
            SecurityAnswer = "Green"
        };

        var createdUser = new UserDto
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            Username = request.Username,
            Password = request.Password,
            Role = RoleType.Seller,
            SecurityQuestion = request.GetSecurityQuestionType(),
            SecurityAnswer = request.SecurityAnswer
        };

        _mockUserService.Setup(s => s.CreateUserAsync(It.IsAny<UserDto>())).ReturnsAsync(createdUser);

        // Act
        var result = await _authController.CreateUser(request);

        // Assert
        var createdResult = Assert.IsType<CreatedResult>(result);
        var responseUser = Assert.IsType<UserResponse>(createdResult.Value);
        Assert.Equal(request.Email, responseUser.Email);
        Assert.Equal(request.Username, responseUser.Username);
    }

    [Fact]
    public async Task CreateUser_ShouldThrowBadRequestException_WhenRoleIsInvalid()
    {
        // Arrange
        var request = new UserCreateRequest
        {
            Email = "admin@example.com",
            Username = "adminUser",
            Password = "securepassword",
            Role = "InvalidRole" // Invalid role
        };

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _authController.CreateUser(request));
    }
    [Fact]
    public async Task CreateUser_ShouldThrowBadRequestException_WhenRoleIsAdmin()
    {
        // Arrange
        var request = new UserCreateRequest
        {
            Email = "admin@example.com",
            Username = "adminUser",
            Password = "securepassword",
            Role = "Admin"
        };

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _authController.CreateUser(request));
    }

    // ===== CREATE SHOPPER TESTS =====

    [Fact]
    public async Task CreateShopper_ShouldReturnCreated_WhenValidRequest()
    {
        // Arrange
        var request = new UserCreateShopperRequest
        {
            Email = "shopper@example.com",
            Username = "shopperUser",
            Password = "securepassword",
            SecurityQuestion = "WhatIsYourFavoriteColor",
            SecurityAnswer = "Blue"
        };

        var createdUser = new UserDto
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            Username = request.Username,
            Password = request.Password,
            Role = RoleType.Shopper,
            SecurityQuestion = request.GetSecurityQuestionType(),
            SecurityAnswer = request.SecurityAnswer
        };

        _mockUserService.Setup(s => s.CreateUserAsync(It.IsAny<UserDto>())).ReturnsAsync(createdUser);

        // Act
        var result = await _authController.CreateShopper(request);

        // Assert
        var createdResult = Assert.IsType<CreatedResult>(result);
        var responseUser = Assert.IsType<UserResponse>(createdResult.Value);
        Assert.Equal(request.Email, responseUser.Email);
        Assert.Equal(request.Username, responseUser.Username);
    }

    [Fact]
    public async Task CreateShopper_ShouldThrowBadRequestException_WhenEmailIsInvalid()
    {
        // Arrange
        var request = new UserCreateShopperRequest
        {
            Email = "invalid-email",
            Username = "shopperUser",
            Password = "securepassword"
        };

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _authController.CreateShopper(request));
    }
}
