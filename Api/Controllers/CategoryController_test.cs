using Api.Controllers;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Moq;
using Services.Interfaces;
using Shared.DTOs;
using Xunit;
using Data.Entities;

public class CategoryController_test
{
    private readonly Mock<ICategoryService> _mockCategoryService;
    private readonly CategoryController _controller;
    private readonly string _adminRole = "Admin";
    private readonly string _sellerRole = "Seller";

    public CategoryController_test()
    {
        _mockCategoryService = new Mock<ICategoryService>();

        _controller = new CategoryController(_mockCategoryService.Object);
    }

    private void SetUserWithRole(string role, string username = "testuser")
    {
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Role, role),
                    new Claim(ClaimTypes.NameIdentifier, username)
                }, "mock"))
            }
        };
    }

    [Fact]
    public async Task GetAllCategories_ShouldReturnOk()
    {
        _mockCategoryService.Setup(s => s.GetAllCategoriesAsync()).ReturnsAsync(new List<CategoryDto>());

        var result = await _controller.GetAllCategories();

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.IsAssignableFrom<IEnumerable<CategoryDto>>(okResult.Value);
    }

    [Fact]
    public async Task GetAvailableCategories_ShouldReturnOk()
    {
        _mockCategoryService.Setup(s => s.GetAvailableCategoriesAsync()).ReturnsAsync(new List<CategoryView>());

        var result = await _controller.GetAvailableCategories();

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.IsAssignableFrom<IEnumerable<CategoryView>>(okResult.Value);
    }

    [Fact]
    public async Task GetCategoryById_ShouldReturnOk_WhenExists()
    {
        _mockCategoryService.Setup(s => s.GetCategoryByIdAsync(1)).ReturnsAsync(new CategoryDto { Id = 1 });

        var result = await _controller.GetCategoryById(1);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var category = Assert.IsType<CategoryDto>(okResult.Value);
        Assert.Equal(1, category.Id);
    }

    [Fact]
    public async Task GetCategoryById_ShouldReturnNotFound_WhenNull()
    {
        _mockCategoryService.Setup(s => s.GetCategoryByIdAsync(99)).ReturnsAsync((CategoryDto?)null);

        var result = await _controller.GetCategoryById(99);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task CreateCategory_ShouldCreateCategory_WhenAdmin()
    {
        SetUserWithRole(_adminRole);
        var request = new CategoryRequest { Name = "Tech" };
        var created = new Category { Id = 2, Name = "Tech", Approved = true };

        _mockCategoryService.Setup(s => s.CreateCategoryAsync(request)).ReturnsAsync(created);

        var result = await _controller.CreateCategory(request);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<CategoryResponse>(okResult.Value);
        Assert.Equal(2, response.Id);
        Assert.Equal("Category created successfully", response.Message);
    }

    [Fact]
    public async Task CreateCategory_ShouldCreateJob_WhenSeller()
    {
        SetUserWithRole(_sellerRole, "employee1");
        var request = new CategoryRequest { Name = "Books", Username = "employee1" };
        var created = new Category { Id = 3, Name = "Books" };

        _mockCategoryService.Setup(s => s.CreateCategoryByEmployeeAsync(request)).ReturnsAsync(created);

        var result = await _controller.CreateCategory(request);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<CategoryResponse>(okResult.Value);
        Assert.Equal(3, response.Id);
        Assert.Equal("Waiting to approve", response.Message);
    }

    [Fact]
    public async Task UpdateCategory_ShouldReturnOk_WhenUpdated()
    {
        var updated = new Category { Id = 1, Name = "Updated" };
        _mockCategoryService.Setup(s => s.UpdateCategoryAsync(1, It.IsAny<CategoryUpdated>())).ReturnsAsync(updated);

        SetUserWithRole(_adminRole);
        var result = await _controller.UpdateCategory(1, new CategoryUpdated());

        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<CategoryResponse>(okResult.Value);
        Assert.Equal("Category updated successfully", response.Message);
    }

    [Fact]
    public async Task DeleteCategory_ShouldReturnOk_WhenAdmin()
    {
        SetUserWithRole(_adminRole);
        _mockCategoryService.Setup(s => s.DeleteCategoryAsync(1)).ReturnsAsync("Category deleted");

        var result = await _controller.DeleteCategory(1, "adminuser");

        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<CategoryDelete>(okResult.Value);
        Assert.Equal("Category deleted", response.Message);
    }

    [Fact]
    public async Task DeleteCategory_ShouldReturnOk_WhenEmployee()
    {
        SetUserWithRole(_sellerRole);
        _mockCategoryService.Setup(s => s.DeleteCategoryByEmployeeAsync(1, "employeeuser"))
            .ReturnsAsync("Job created for category deletion");

        var result = await _controller.DeleteCategory(1, "employeeuser");

        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ProductDelete>(okResult.Value);
        Assert.Equal("Job created for category deletion", response.message);
    }
}