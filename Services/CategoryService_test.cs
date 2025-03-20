using Data;
using Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Services;
using Services.Interfaces;
using Xunit;

public class CategoryService_test
{
    private readonly VinnareDbContext _dbContext;
    private readonly Mock<ILogger<CategoryService>> _mockLogger;
    private readonly Mock<IJobService> _mockJobService;
    private readonly Mock<IUserService> _mockUserService;
    private readonly CategoryService _categoryService;

    public CategoryService_test()
    {
        var options = new DbContextOptionsBuilder<VinnareDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase_Category")
            .Options;

        _dbContext = new VinnareDbContext(options);
        _dbContext.Database.EnsureCreated();

        _mockLogger = new Mock<ILogger<CategoryService>>();
        _mockJobService = new Mock<IJobService>();
        _mockUserService = new Mock<IUserService>();

        _categoryService = new CategoryService(_dbContext, _mockLogger.Object, _mockJobService.Object, _mockUserService.Object);
    }


    [Fact]
    public async Task ApproveCategory_ShouldSetApproved_WhenCategoryExists()
    {
        // Arrange
        var category = new Category { Id = 1, Name = "fake", Approved = false };
        _dbContext.Categories.Add(category);
        _dbContext.SaveChanges();

        // Act
        Console.WriteLine("DO print work?");
        var aaa = await _dbContext.Categories.FindAsync(1);
        Console.WriteLine(aaa.ToString());
        await _categoryService.ApproveCategory(1, true);
        var updatedCategory = await _dbContext.Categories.FindAsync(1);

        // Assert
        Assert.NotNull(updatedCategory);
        Assert.True(updatedCategory.Approved);
    }

    [Fact]
    public async Task ApproveCategory_ShouldThrowException_WhenCategoryDoesNotExist()
    {
        // Act & Assert
        await Assert.ThrowsAsync<NullReferenceException>(() => _categoryService.ApproveCategory(999, true));
    }
}
