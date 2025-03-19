using Data;
using Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Services;
using Xunit;

public class CategoryService_test
{
    private readonly VinnareDbContext _dbContext;
    private readonly Mock<ILogger<CategoryService>> _mockLogger;
    private readonly CategoryService _categoryService;

    public CategoryService_test()
    {
        var options = new DbContextOptionsBuilder<VinnareDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase_Category")
            .Options;

        _dbContext = new VinnareDbContext(options);
        _dbContext.Database.EnsureCreated();

        _mockLogger = new Mock<ILogger<CategoryService>>();
        _categoryService = new CategoryService(_dbContext, _mockLogger.Object);
    }

    [Fact]
    public async Task ApproveCategory_ShouldSetApproved_WhenCategoryExists()
    {
        // Arrange
        var category = new Category { Id = 1, Name = "fake", Approved = false };
        _dbContext.Categories.Add(category);
        _dbContext.SaveChanges();

        // Act
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
