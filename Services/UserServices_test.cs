using Data;
using Data.Entities;
using Microsoft.EntityFrameworkCore;
using Moq;
using Services;
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
        var options = new DbContextOptionsBuilder<VinnareDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase")
            .Options;

        _dbContext = new VinnareDbContext(options);
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
            Role = RoleType.Seller
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
            Role = RoleType.Seller
        };

        // Act
        await _userService.CreateUserAsync(newUser);

        // Assert
        _mockPasswordHasher.Verify(p => p.HashPassword("passwordToHash"), Times.Once);
    }
}
