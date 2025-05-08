using Data;
using Data.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using Services;
using Services.Interfaces;
using Services.Utils;
using Shared.DTOs;
using Shared.Enums;
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
        _dbContext = TestDbContextFactory.Create();
        _dbContext.Database.EnsureCreated();

        _mockLogger = new Mock<ILogger<CategoryService>>();
        _mockJobService = new Mock<IJobService>();
        _mockUserService = new Mock<IUserService>();

        _categoryService = new CategoryService(_dbContext, _mockLogger.Object, _mockJobService.Object, _mockUserService.Object);
    }

    [Fact]
    public async Task GetAllCategoriesAsync_ShouldReturnAll()
    {
        _dbContext.Categories.Add(new Category { Name = "Tech", ImageUrl = "tech.jpg" });
        await _dbContext.SaveChangesAsync();

        var result = await _categoryService.GetAllCategoriesAsync();

        Assert.Single(result);
        Assert.Equal("Tech", result.First().Name);
    }

    [Fact]
    public async Task GetAvailableCategoriesAsync_ShouldReturnApproved()
    {
        _dbContext.Categories.AddRange(
            new Category { Name = "Toys", Approved = true, ImageUrl = "toys.jpg" },
            new Category { Name = "Games", Approved = false, ImageUrl = "games.jpg" }
        );
        await _dbContext.SaveChangesAsync();

        var result = await _categoryService.GetAvailableCategoriesAsync();

        Assert.Single(result);
        Assert.Equal("Toys", result.First().Name);
    }

    [Fact]
    public async Task GetCategoryByIdAsync_ShouldReturnCorrectCategory()
    {
        var category = new Category { Id = 10, Name = "Food", ImageUrl = "food.jpg" };
        _dbContext.Categories.Add(category);
        await _dbContext.SaveChangesAsync();

        var result = await _categoryService.GetCategoryByIdAsync(10);

        Assert.NotNull(result);
        Assert.Equal("Food", result!.Name);
    }

    [Fact]
    public async Task GetCategoryNameByIdAsync_ShouldReturnName()
    {
        var category = new Category { Id = 99, Name = "Books", ImageUrl = "books.jpg" };
        _dbContext.Categories.Add(category);
        await _dbContext.SaveChangesAsync();

        var result = await _categoryService.GetCategoryNameByIdAsync(99);

        Assert.Equal("Books", result);
    }

    [Fact]
    public async Task CreateCategoryByEmployeeAsync_ShouldCreateWithJob()
    {
        var userId = Guid.NewGuid();
        _mockUserService.Setup(u => u.GetIdByUsername("employee")).ReturnsAsync(userId);

        // Add user to database
        _dbContext.Users.Add(new User { Id = userId, Username = "employee", Role = RoleType.Seller });
        await _dbContext.SaveChangesAsync();

        var request = new CategoryRequest { Name = "NewCat", Username = "employee", ImageUrl = "newcat.jpg" };

        var result = await _categoryService.CreateCategoryByEmployeeAsync(request);

        Assert.NotNull(result);
        Assert.Equal("NewCat", result.Name);
        Assert.False(result.Approved);
        _mockJobService.Verify(j => j.CreateJobAsync(It.IsAny<JobDto>()), Times.Once);
    }

    [Fact]
    public async Task CreateCategoryAsync_ShouldCreateDirectly()
    {
        var request = new CategoryRequest { Name = "AdminCat", ImageUrl = "admincat.jpg" };

        var result = await _categoryService.CreateCategoryAsync(request);

        Assert.NotNull(result);
        Assert.Equal("AdminCat", result.Name);
        Assert.True(result.Approved);
    }

    [Fact]
    public async Task UpdateCategoryAsync_ShouldUpdateName()
    {
        var category = new Category { Name = "Old", Approved = true, ImageUrl = "old.jpg" };
        _dbContext.Categories.Add(category);
        await _dbContext.SaveChangesAsync();

        var updated = await _categoryService.UpdateCategoryAsync(category.Id, new CategoryUpdated { Name = "Updated" });

        Assert.Equal("Updated", updated.Name);
    }

    [Fact]
    public async Task DeleteCategoryAsync_ShouldDelete()
    {
        var category = new Category { Name = "ToDelete", Approved = true, ImageUrl = "todelete.jpg" };
        _dbContext.Categories.Add(category);
        await _dbContext.SaveChangesAsync();

        var result = await _categoryService.DeleteCategoryAsync(category.Id);

        Assert.Equal("Category deleted successfully", result);
        Assert.Null(await _dbContext.Categories.FindAsync(category.Id));
    }

    [Fact]
    public async Task DeleteCategoryByEmployeeAsync_ShouldCreateJobOnly()
    {
        var userId = Guid.NewGuid();
        var category = new Category { Name = "SoftDelete", Approved = true, ImageUrl = "softdelete.jpg" };
        _dbContext.Categories.Add(category);
        await _dbContext.SaveChangesAsync();

        _mockUserService.Setup(u => u.GetIdByUsername("employee")).ReturnsAsync(userId);

        var result = await _categoryService.DeleteCategoryByEmployeeAsync(category.Id, "employee");

        Assert.Equal("Job created, waiting the admin approve.", result);
        _mockJobService.Verify(j => j.CreateJobAsync(It.Is<JobDto>(j => j.CategoryId == category.Id)), Times.Once);
    }

    [Fact]
    public async Task ApproveCategory_ShouldSetApproved_WhenCategoryExists()
    {
        var category = new Category { Id = 1, Name = "fake", Approved = false, ImageUrl = "fake.jpg" };
        _dbContext.Categories.Add(category);
        _dbContext.SaveChanges();

        await _categoryService.ApproveCategory(1, true);
        var updatedCategory = await _dbContext.Categories.FindAsync(1);

        Assert.NotNull(updatedCategory);
        Assert.True(updatedCategory.Approved);
    }

    [Fact]
    public async Task ApproveCategory_ShouldThrowException_WhenCategoryDoesNotExist()
    {
        await Assert.ThrowsAsync<NullReferenceException>(() => _categoryService.ApproveCategory(999, true));
    }
}