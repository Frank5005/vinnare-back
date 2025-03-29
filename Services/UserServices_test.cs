using Data;
using Data.Entities;
using Microsoft.EntityFrameworkCore;
using Moq;
using Services;
using Services.Utils;
using Shared.DTOs;
using Shared.Enums;
using Xunit;

public class UserService_test
{
    private readonly VinnareDbContext _dbContext;
    private readonly Mock<IPasswordHasher> _mockPasswordHasher;
    private readonly UserService _userService;

    public UserService_test()
    {
        _dbContext = TestDbContextFactory.Create();
        _dbContext.Database.EnsureCreated();

        _dbContext.Users.Add(new User
        {
            Id = Guid.NewGuid(),
            Email = "test1@example.com",
            Username = "testuser1",
            Password = "hashedpassword",
            Role = RoleType.Admin
        });

        _dbContext.SaveChanges();

        _mockPasswordHasher = new Mock<IPasswordHasher>();
        _mockPasswordHasher.Setup(p => p.HashPassword(It.IsAny<string>()))
                           .Returns("hashedPassword123");

        _userService = new UserService(_dbContext, _mockPasswordHasher.Object);
    }

    [Fact]
    public async Task GetAllUsersAsync_ShouldReturnUsers()
    {

        _dbContext.Database.EnsureDeleted();
        _dbContext.Database.EnsureCreated();

        _dbContext.Users.Add(new User
        {
            Id = Guid.NewGuid(),
            Email = "test1@example.com",
            Username = "testuser1",
            Password = "hashedpassword",
            Role = RoleType.Admin
        });

        _dbContext.SaveChanges();
        // Act
        var users = await _userService.GetAllUsersAsync();

        // Assert
        Assert.NotNull(users);
        Assert.Single(users);
    }

    [Fact]
    public async Task GetUserByIdAsync_ShouldReturnCorrectUser()
    {
        // Arrange
        var testUser = await _dbContext.Users.FirstAsync();

        // Act
        var result = await _userService.GetUserByIdAsync(testUser.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(testUser.Email, result.Email);
    }

    [Fact]
    public async Task CreateUserAsync_ShouldAddUser()
    {
        // Arrange
        var newUser = new UserDto
        {
            Email = "newuser@example.com",
            Username = "newuser",
            Password = "password123",
            Role = RoleType.Seller,
            Address = "123 Main St"
        };

        // Act
        var createdUser = await _userService.CreateUserAsync(newUser);

        // Assert
        var userInDb = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == "newuser@example.com");
        Assert.NotNull(userInDb);
        Assert.Equal(newUser.Email, createdUser.Email);
    }

    [Fact]
    public async Task CreateUserAsync_ShouldHashPassword()
    {
        // Arrange
        var newUser = new UserDto
        {
            Email = "hashuser@example.com",
            Username = "hashuser",
            Password = "passwordToHash",
            Role = RoleType.Seller,
            Address = "123 Main St"
        };

        // Act
        await _userService.CreateUserAsync(newUser);

        // Assert
        _mockPasswordHasher.Verify(p => p.HashPassword("passwordToHash"), Times.Once);
    }

    [Fact]
    public async Task GetUserByUsername_ShouldReturnCorrectUser()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Database.EnsureCreated();

        _dbContext.Users.Add(new User
        {
            Id = Guid.NewGuid(),
            Email = "test1@example.com",
            Username = "testuser1",
            Password = "hashedpassword",
            Role = RoleType.Admin
        });

        _dbContext.SaveChanges();
        // Arrange
        var testUser = await _dbContext.Users.FirstAsync();

        // Act
        var result = await _userService.GetUserByUsername(testUser.Username);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(testUser.Username, result.Username);
        Assert.Equal(testUser.Email, result.Email);
    }

    [Fact]
    public async Task GetUserByUsername_ShouldReturnNull_WhenUserDoesNotExist()
    {
        // Act
        var result = await _userService.GetUserByUsername("nonexistentuser");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetIdByUsername_ShouldReturnCorrectId()
    {
        // Arrange
        var testUser = await _dbContext.Users.FirstAsync();

        // Act
        var result = await _userService.GetIdByUsername(testUser.Username);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(testUser.Id, result);
    }

    [Fact]
    public async Task GetIdByUsername_ShouldReturnNull_WhenUserDoesNotExist()
    {
        // Act
        var result = await _userService.GetIdByUsername("nonexistentuser");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateUserAsync_ShouldUpdateFields()
    {
        // Arrange
        var testUser = await _dbContext.Users.FirstAsync();
        var updateRequest = new UserDto
        {
            Email = "updated@example.com",
            Password = "newpassword",
            Name = "Updated Name"
        };

        _mockPasswordHasher.Setup(p => p.HashPassword("newpassword")).Returns("hashedNewPassword");

        // Act
        var updatedUser = await _userService.UpdateUserAsync(testUser.Id, updateRequest);

        // Assert
        Assert.NotNull(updatedUser);
        Assert.Equal("updated@example.com", updatedUser.Email);
        Assert.Equal("Updated Name", updatedUser.Name);
        Assert.NotEqual(testUser.Password, updatedUser.Password); // Ensure password was changed
    }

    [Fact]
    public async Task UpdateUserAsync_ShouldReturnNull_WhenUserDoesNotExist()
    {
        // Arrange
        var updateRequest = new UserDto
        {
            Email = "updated@example.com",
            Password = "newpassword"
        };

        // Act
        var result = await _userService.UpdateUserAsync(Guid.NewGuid(), updateRequest);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteUsersAsync_ShouldDeleteUsers()
    {
        // Arrange
        var testUser1 = new User
        {
            Id = Guid.NewGuid(),
            Email = "delete1@example.com",
            Username = "deleteuser1",
            Password = "password",
            Role = RoleType.Shopper
        };

        var testUser2 = new User
        {
            Id = Guid.NewGuid(),
            Email = "delete2@example.com",
            Username = "deleteuser2",
            Password = "password",
            Role = RoleType.Seller
        };

        _dbContext.Users.AddRange(testUser1, testUser2);
        await _dbContext.SaveChangesAsync();

        var usernamesToDelete = new List<string> { "deleteuser1", "deleteuser2" };

        // Act
        var deletedUsers = await _userService.DeleteUsersAsync(usernamesToDelete);

        // Assert
        Assert.Equal(2, deletedUsers.Count);
        Assert.DoesNotContain(await _dbContext.Users.ToListAsync(), u => usernamesToDelete.Contains(u.Username));
    }

    [Fact]
    public async Task DeleteUsersAsync_ShouldReturnEmptyList_WhenUsersNotFound()
    {
        // Arrange
        var usernamesToDelete = new List<string> { "nonexistentuser1", "nonexistentuser2" };

        // Act
        var deletedUsers = await _userService.DeleteUsersAsync(usernamesToDelete);

        // Assert
        Assert.Empty(deletedUsers);
    }
}

