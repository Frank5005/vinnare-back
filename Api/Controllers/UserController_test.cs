using System.Security.Claims;
using Api.Controllers;
using Api.DTOs;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Services.Interfaces;
using Shared.DTOs;
using Shared.Exceptions;
using Xunit;

public class UserController_test
{
    private readonly Mock<IUserService> _mockUserService;
    private readonly Mock<ITokenService> _mockTokenService;
    private readonly UserController _userController;

    public UserController_test()
    {
        _mockUserService = new Mock<IUserService>();
        _mockTokenService = new Mock<ITokenService>();
        _userController = new UserController(_mockUserService.Object, _mockTokenService.Object);
    }


    private void SetUserContext(string username)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, username)
        };

        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var principal = new ClaimsPrincipal(identity);

        _userController.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };
    }


    // ===== GET ALL USERS TEST =====

    [Fact]
    public async Task GetAllUsers_ShouldReturnOk_WithUserList()
    {
        // Arrange
        var users = new List<UserDtoString>
        {
            new UserDtoString { Username = "user1", Email = "user1@example.com", Role = "Admin" },
            new UserDtoString { Username = "user2", Email = "user2@example.com", Role = "Customer" }
        };

        _mockUserService.Setup(s => s.GetAllUsersAsync()).ReturnsAsync(users);

        // Act
        var result = await _userController.GetAllUsers();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedUsers = Assert.IsType<List<UserDtoString>>(okResult.Value);
        Assert.Equal(2, returnedUsers.Count);
    }

    // ===== UPDATE USER TESTS =====

    /*
    [Fact]
    public async Task UpdateUser_ShouldReturnOk_WhenUserIsUpdated()
    {
        // Arrange
        var updateRequest = new UpdateUserRequest
        {
            Username = "existinguser",
            Email = "newemail@example.com",
            Name = "Updated Name"
        };

        var userId = Guid.NewGuid();
        _mockUserService.Setup(s => s.GetIdByUsername(updateRequest.Username)).ReturnsAsync(userId);
        _mockUserService.Setup(s => s.UpdateUserAsync(userId, It.IsAny<UserDto>())).ReturnsAsync(new UserDto
        {
            Id = userId,
            Username = "existinguser",
            Email = "newemail@example.com",
            Name = "Updated Name"
        });

        // Act
        var result = await _userController.UpdateUser(updateRequest);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<DefaultResponse>(okResult.Value);
        Assert.Equal("User existinguser has been updated successfully", response.message);
    }

    [Fact]
    public async Task UpdateUser_ShouldReturnNotFound_WhenUserDoesNotExist()
    {
        // Arrange
        var updateRequest = new UpdateUserRequest { Username = "nonexistentuser" };

        _mockUserService.Setup(s => s.GetIdByUsername(updateRequest.Username)).ReturnsAsync((Guid?)null);

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _userController.UpdateUser(updateRequest));
    }
    */

    [Fact]
    public async Task UpdateUser_ShouldReturnBadRequest_WhenRequestIsNull()
    {
        // Act
        var result = await _userController.UpdateUser(null);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("User data is required.", badRequestResult.Value);
    }

    // ===== DELETE USERS TESTS =====

    [Fact]
    public async Task DeleteUsers_ShouldReturnNoContent_WhenUsersAreDeleted()
    {
        // Arrange
        var deleteRequest = new DeleteUserRequest { Users = new List<string> { "user1", "user2" } };
        var deletedUsers = new List<UserDto>
        {
            new UserDto { Username = "user1" },
            new UserDto { Username = "user2" }
        };

        _mockUserService.Setup(s => s.DeleteUsersAsync(deleteRequest.Users)).ReturnsAsync(deletedUsers);

        // Act
        var result = await _userController.DeleteUsers(deleteRequest);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task DeleteUsers_ShouldReturnNotFound_WhenUsersNotFound()
    {
        // Arrange
        var deleteRequest = new DeleteUserRequest { Users = new List<string> { "user1", "user2" } };

        _mockUserService.Setup(s => s.DeleteUsersAsync(deleteRequest.Users)).ReturnsAsync(new List<UserDto>());

        // Act
        var result = await _userController.DeleteUsers(deleteRequest);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("No matching users found to delete.", notFoundResult.Value);
    }

    [Fact]
    public async Task DeleteUsers_ShouldReturnBadRequest_WhenRequestIsNull()
    {
        // Act
        var result = await _userController.DeleteUsers(null);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("At least one username must be provided.", badRequestResult.Value);
    }

    // ===== UPDATE SHOPPER TESTS =====

    [Fact]
    public async Task UpdateShopper_ShouldReturnOk_WhenUserIsUpdated()
    {
        // Arrange
        string tokenUsername = "shopperUser";
        SetUserContext(tokenUsername); // Simulate token-based authentication

        var updateRequest = new UpdateShoppperRequest
        {
            Email = "newemail@example.com",
            Name = "Updated Shopper"
        };

        var userId = Guid.NewGuid();
        _mockUserService.Setup(s => s.GetIdByUsername(tokenUsername)).ReturnsAsync(userId);
        _mockUserService.Setup(s => s.UpdateUserAsync(userId, It.IsAny<UserDto>())).ReturnsAsync(new UserDto
        {
            Id = userId,
            Username = tokenUsername,
            Email = "newemail@example.com",
            Name = "Updated Shopper"
        });

        // Act
        var result = await _userController.UpdateShopper(updateRequest);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<DefaultResponse>(okResult.Value);
        Assert.Equal($"User {tokenUsername} has been updated successfully", response.message);
    }

    [Fact]
    public async Task UpdateShopper_ShouldReturnUnauthorized_WhenTokenHasNoUsername()
    {
        // Arrange
        _userController.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal() } // No claims
        };

        var updateRequest = new UpdateShoppperRequest { Email = "newemail@example.com" };

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedException>(() => _userController.UpdateShopper(updateRequest));
    }

    [Fact]
    public async Task UpdateShopper_ShouldReturnNotFound_WhenUserDoesNotExist()
    {
        // Arrange
        string tokenUsername = "nonexistentuser";
        SetUserContext(tokenUsername);

        _mockUserService.Setup(s => s.GetIdByUsername(tokenUsername)).ReturnsAsync((Guid?)null);

        var updateRequest = new UpdateShoppperRequest { Email = "newemail@example.com" };

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _userController.UpdateShopper(updateRequest));
    }

    [Fact]
    public async Task UpdateShopper_ShouldReturnBadRequest_WhenRequestIsNull()
    {
        // Arrange
        string tokenUsername = "shopperUser";
        SetUserContext(tokenUsername);

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _userController.UpdateShopper(null));
    }

    // ===== DELETE SHOPPER TESTS =====

    [Fact]
    public async Task DeleteShopper_ShouldReturnNoContent_WhenUserIsDeleted()
    {
        // Arrange
        string tokenUsername = "shopperUser";
        SetUserContext(tokenUsername);

        var deletedUsers = new List<UserDto>
        {
            new UserDto { Username = tokenUsername }
        };

        _mockUserService.Setup(s => s.DeleteUsersAsync(It.Is<List<string>>(u => u.Contains(tokenUsername))))
                        .ReturnsAsync(deletedUsers);

        // Act
        var result = await _userController.DeleteShopper();

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task DeleteShopper_ShouldReturnUnauthorized_WhenTokenHasNoUsername()
    {
        // Arrange
        _userController.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal() } // No claims
        };

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedException>(() => _userController.DeleteShopper());
    }

    [Fact]
    public async Task DeleteShopper_ShouldReturnNotFound_WhenUserDoesNotExist()
    {
        // Arrange
        string tokenUsername = "nonexistentuser";
        SetUserContext(tokenUsername);

        _mockUserService.Setup(s => s.DeleteUsersAsync(It.IsAny<List<string>>())).ReturnsAsync(new List<UserDto>());

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _userController.DeleteShopper());
    }
}
