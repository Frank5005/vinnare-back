using Xunit;
using Moq;
using Services;
using Data;
using Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Services.Interfaces;
using Shared.DTOs;
using Shared.Enums;
using Services.Utils;
using Shared.Exceptions;

public class ProductService_test
{
    private readonly VinnareDbContext _dbContext;
    private readonly Mock<ILogger<ProductService>> _mockLogger;
    private readonly Mock<IJobService> _mockJobService;
    private readonly Mock<IUserService> _mockUserService;
    private readonly Mock<IReviewService> _mockReviewService;
    private readonly ProductService _productService;


    public ProductService_test()
    {
        _dbContext = TestDbContextFactory.Create();
        _dbContext.Database.EnsureCreated();

        _mockLogger = new Mock<ILogger<ProductService>>();
        _mockJobService = new Mock<IJobService>();
        _mockUserService = new Mock<IUserService>();
        _mockReviewService = new Mock<IReviewService>();

        _productService = new ProductService(_dbContext, _mockLogger.Object, _mockJobService.Object, _mockUserService.Object, _mockReviewService.Object);
    }

    [Fact]
    public async Task ApproveProduct_ShouldSetApprovedToTrue()
    {
        var product = new Product { Id = 1, Title = "Test", Approved = false };
        _dbContext.Products.Add(product);
        await _dbContext.SaveChangesAsync();

        await _productService.ApproveProduct(1, true);

        var updated = await _dbContext.Products.FindAsync(1);
        Assert.True(updated.Approved);
    }

    [Fact]
    public async Task GetAllProducts_ShouldReturnAll()
    {
        _dbContext.Products.AddRange(
            new Product { Id = 1, Title = "P1", Approved = true },
            new Product { Id = 2, Title = "P2", Approved = false }
        );
        await _dbContext.SaveChangesAsync();

        var result = await _productService.GetAllProductsAsync();

        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetAvailableProductsPage_ShouldReturnPaginatedList()
    {
        for (int i = 1; i <= 10; i++)
        {
            _dbContext.Products.Add(new Product { Title = $"P{i}", Approved = true, Available = i });
        }
        await _dbContext.SaveChangesAsync();

        var result = await _productService.GetAvailableProductsPageAsync();

        Assert.Equal(10, result.Count()); // 6 productos por página
    }

    [Fact]
    public async Task GetProductsByCategoryAsync_ShouldReturnMatchingProducts()
    {
        _dbContext.Products.AddRange(
            new Product { Id = 1, CategoryId = 5, Title = "A", Approved = true, Available = 3 },
            new Product { Id = 2, CategoryId = 7, Title = "B", Approved = true, Available = 5 }
        );
        await _dbContext.SaveChangesAsync();

        var result = await _productService.GetProductsByCategoryAsync(5);

        Assert.Single(result);
        Assert.Equal("A", result.First().Title);
    }

    [Fact]
    public async Task GetProductByIdAsync_ShouldReturnDetailedProduct()
    {
        var ownerId = Guid.NewGuid();
        var product = new Product
        {
            Id = 3,
            OwnerId = ownerId,
            Title = "Pro",
            Category = "Cat",
            Owner = new User { Username = "user" },
            Available = 3,
            Approved = true
        };
        _dbContext.Products.Add(product);
        await _dbContext.SaveChangesAsync();

        _mockReviewService.Setup(r => r.GetReviewsRateByIdAsync(3)).ReturnsAsync(5);

        var result = await _productService.GetProductByIdAsync(3);

        Assert.Equal("Pro", result.Title);
        Assert.Equal("Cat", result.Category);
        //Assert.Equal("user", result.Username);
        Assert.Equal(5, result.Rate);
    }

    [Fact]
    public async Task CreateProductAsync_Admin_ShouldFail_WhenUserNotExist()
    {
        var fakeUserId = Guid.NewGuid();

        var productDto = new ProductRequest
        {
            OwnerId = fakeUserId,
            Title = "Nuevo",
            Price = 10,
            Category = "NuevaCat",
            Quantity = 20,
            Available = 10
        };

        _mockUserService.Setup(u => u.GetUsernameById(fakeUserId)).ReturnsAsync((string?)null);

        await Assert.ThrowsAsync<Exception>(() =>
            _productService.CreateProductAsync(productDto));

        var product = await _dbContext.Products.FirstOrDefaultAsync(p => p.Title == "Nuevo");
        Assert.Null(product);
    }

    [Fact]
    public async Task CreateProductAsync_Seller_ShouldFail_WhenUserNotExist()
    {
        var fakeUserId = Guid.NewGuid();

        var productDto = new ProductRequest
        {
            OwnerId = fakeUserId,
            Title = "JobProduct",
            Price = 10,
            Category = "Cat",
            Quantity = 10,
            Available = 5
        };

        _mockUserService.Setup(u => u.GetUsernameById(fakeUserId)).ReturnsAsync((string?)null);

        await Assert.ThrowsAsync<Exception>(() =>
            _productService.CreateProductByEmployeeAsync(productDto));

        var product = await _dbContext.Products.FirstOrDefaultAsync(p => p.Title == "JobProduct");
        Assert.Null(product);
    }

    /*
    [Fact]
    public async Task UpdateProductAsync_ShouldFail_WhenCategoryNotExist()
    {
        var product = new Product
        {
            Id = 5,
            Title = "Old",
            Price = 10,
            Approved = true
        };
        _dbContext.Products.Add(product);
        await _dbContext.SaveChangesAsync();

        var update = new ProductUpdate
        {
            Title = "New",
            Price = 25,
            Category = "Inexistente"
        };

        _mockCategoryService
            .Setup(c => c.GetCategoryByNameAsync("Inexistente"))
            .ReturnsAsync((CategoryDto?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _productService.UpdateProductAsync(5, update));

        // Verifica que NO se actualizó el producto
        var unchanged = await _dbContext.Products.FindAsync(5);
        Assert.Equal("Old", unchanged!.Title);
        Assert.Equal(10, unchanged.Price);
    }
    */

    [Fact]
    public async Task DeleteProductAsync_Admin_ShouldRemoveDirectly()
    {
        _dbContext.Products.Add(new Product { Id = 8, Title = "DeleteMe" });
        await _dbContext.SaveChangesAsync();

        await _productService.DeleteProductAsync(8);

        var result = await _dbContext.Products.FindAsync(8);
        Assert.Null(result);
    }


    /*
    [Fact]
    public async Task DeleteProductAsync_Seller_ShouldCreateJob()
    {
        _dbContext.Products.Add(new Product { Id = 9, Title = "JobDel", OwnerId = Guid.NewGuid() });
        await _dbContext.SaveChangesAsync();

        await _productService.DeleteProductAsync(9);

        _mockJobService.Verify(j => j.CreateJobAsync(It.Is<JobDto>(job =>
            job.Type == JobType.Product &&
            job.Operation == OperationType.Delete)), Times.Once);
    }
    */
}
