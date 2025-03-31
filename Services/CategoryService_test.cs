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
        _mockLogger = new Mock<ILogger<CategoryService>>();
        _mockJobService = new Mock<IJobService>();
        _mockUserService = new Mock<IUserService>();

        _categoryService = new CategoryService(
            _dbContext,
            _mockLogger.Object,
            _mockJobService.Object,
            _mockUserService.Object);
    }

    [Fact]
    public async Task GetAllCategoriesAsync_ShouldReturnAll()
    {
        _dbContext.Categories.Add(new Category { Name = "Test" });
        await _dbContext.SaveChangesAsync();

        var result = await _categoryService.GetAllCategoriesAsync();

        Assert.Single(result);
        Assert.Equal("Test", result.First().Name);
    }

    [Fact]
    public async Task GetAvailableCategoriesAsync_ShouldReturnApproved()
    {
        _dbContext.Categories.Add(new Category { Name = "A", Approved = true });
        _dbContext.Categories.Add(new Category { Name = "B", Approved = false });
        await _dbContext.SaveChangesAsync();

        var result = await _categoryService.GetAvailableCategoriesAsync();

        Assert.Single(result);
        Assert.Equal("A", result.First().Name);
    }

    [Fact]
    public async Task GetCategoryByIdAsync_ShouldReturnCategory_WhenExists()
    {
        var cat = new Category { Name = "SearchMe" };
        _dbContext.Categories.Add(cat);
        await _dbContext.SaveChangesAsync();

        var result = await _categoryService.GetCategoryByIdAsync(cat.Id);

        Assert.NotNull(result);
        Assert.Equal("SearchMe", result.Name);
    }

    [Fact]
    public async Task CreateCategoryByEmployeeAsync_ShouldCreateCategoryAndJob()
    {
        var employeeId = Guid.NewGuid();
        _mockUserService.Setup(u => u.GetIdByUsername("employee"))
            .ReturnsAsync(employeeId);

        var result = await _categoryService.CreateCategoryByEmployeeAsync(new CategoryRequest
        {
            Name = "Gaming",
            Username = "employee"
        });

        Assert.False(result.Approved);
        _mockJobService.Verify(j => j.CreateJobAsync(It.Is<JobDto>(j => j.Type == JobType.Category && j.Operation == OperationType.Create)), Times.Once);
    }

    [Fact]
    public async Task CreateCategoryAsync_ShouldCreateApprovedCategory()
    {
        var result = await _categoryService.CreateCategoryAsync(new CategoryRequest
        {
            Name = "Food"
        });

        Assert.True(result.Approved);
        Assert.Equal("Food", result.Name);
    }

    [Fact]
    public async Task UpdateCategoryAsync_ShouldUpdateSuccessfully()
    {
        _dbContext.Categories.Add(new Category { Name = "Original", Approved = false });
        await _dbContext.SaveChangesAsync();

        var category = _dbContext.Categories.First();

        var result = await _categoryService.UpdateCategoryAsync(category.Id, new CategoryUpdated
        {
            Name = "Updated",
            Approved = true
        });

        Assert.Equal("Updated", result.Name);
        Assert.True(result.Approved);
    }

    [Fact]
    public async Task DeleteCategoryAsync_ShouldRemoveCategory()
    {
        var cat = new Category { Name = "ToDelete" };
        _dbContext.Categories.Add(cat);
        await _dbContext.SaveChangesAsync();

        var result = await _categoryService.DeleteCategoryAsync(cat.Id);

        Assert.Equal("Category deleted successfully", result);
        Assert.Null(await _dbContext.Categories.FindAsync(cat.Id));
    }

    [Fact]
    public async Task DeleteCategoryByEmployeeAsync_ShouldCreateDeleteJob()
    {
        var cat = new Category { Name = "ToTrack" };
        _dbContext.Categories.Add(cat);
        await _dbContext.SaveChangesAsync();

        var userId = Guid.NewGuid();
        _mockUserService.Setup(u => u.GetIdByUsername("mod"))
            .ReturnsAsync(userId);

        var result = await _categoryService.DeleteCategoryByEmployeeAsync(cat.Id, "mod");

        Assert.Equal("You can't delete a category.", result);
        _mockJobService.Verify(j => j.CreateJobAsync(It.Is<JobDto>(j => j.CategoryId == cat.Id && j.Operation == OperationType.Delete)), Times.Once);
    }

    [Fact]
    public async Task ApproveCategory_ShouldSetApproved()
    {
        var cat = new Category { Name = "Unapproved", Approved = false };
        _dbContext.Categories.Add(cat);
        await _dbContext.SaveChangesAsync();

        await _categoryService.ApproveCategory(cat.Id, true);

        var result = await _dbContext.Categories.FindAsync(cat.Id);
        Assert.True(result.Approved);
    }
}
