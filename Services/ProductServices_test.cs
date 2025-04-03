using Data;
using Data.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using Services;
using Services.Interfaces;
using Services.Utils;
using Shared.DTOs;
using Shared.Enums;
using Shared.Exceptions;
using Xunit;

public class ProductService_test
{
    private readonly VinnareDbContext _dbContext;
    private readonly Mock<ILogger<ProductService>> _mockLogger;
    private readonly Mock<IJobService> _mockJobService;
    private readonly Mock<IUserService> _mockUserService;
    private readonly Mock<IReviewService> _mockReviewService;
    private readonly Mock<IServiceProvider> _mockServiceProvider;
    private readonly Mock<ICategoryService> _mockCategoryService;
    private readonly ProductService _productService;

    public ProductService_test()
    {
        _dbContext = TestDbContextFactory.Create();
        _mockLogger = new Mock<ILogger<ProductService>>();
        _mockJobService = new Mock<IJobService>();
        _mockUserService = new Mock<IUserService>();
        _mockReviewService = new Mock<IReviewService>();
        _mockServiceProvider = new Mock<IServiceProvider>();
        _mockCategoryService = new Mock<ICategoryService>();
        _mockServiceProvider.Setup(sp => sp.GetService(typeof(ICategoryService)))
            .Returns(_mockCategoryService.Object);
        _mockCategoryService.Setup(c => c.CheckAvailableCategory("NonExisting"))
            .ThrowsAsync(new NotFoundException("The category doesn't exists."));

        _productService = new ProductService(_dbContext, _mockLogger.Object, _mockJobService.Object, _mockUserService.Object, _mockReviewService.Object, _mockServiceProvider.Object);
    }


    [Fact]
    public async Task GetAvailableProductsAsync_ShouldReturnOnlyApprovedWithStock()
    {
        _dbContext.Products.Add(new Product { Title = "A", Price = 10, Approved = true, Available = 5, Category = "A", OwnerId = Guid.NewGuid() });
        _dbContext.Products.Add(new Product { Title = "B", Price = 10, Approved = false, Available = 5, Category = "B", OwnerId = Guid.NewGuid() });
        await _dbContext.SaveChangesAsync();

        var result = await _productService.GetAvailableProductsPageAsync();

        Assert.Single(result);
        Assert.Equal("A", result.First().Title);
    }

    [Fact]
    public async Task GetProductByIdAsync_ShouldReturnProductDto_WhenExists()
    {
        var product = new Product { Title = "Test", Price = 10, Category = "C", OwnerId = Guid.NewGuid() };
        _dbContext.Products.Add(product);
        await _dbContext.SaveChangesAsync();

        var result = await _productService.GetProductByIdAsync(product.Id);

        Assert.NotNull(result);
        Assert.Equal(product.Title, result.Title);
    }

    [Fact]
    public async Task GetProductByIdAsync_ShouldReturnNull_WhenNotExists()
    {
        var result = await _productService.GetProductByIdAsync(999);
        Assert.Null(result);
    }

    [Fact]
    public async Task CreateProductAsync_ShouldThrow_WhenOwnerNotExists()
    {
        _dbContext.Categories.Add(new Category { Name = "Books" });
        await _dbContext.SaveChangesAsync();

        var request = new ProductRequest { OwnerId = Guid.NewGuid(), Category = "Books" };

        await Assert.ThrowsAsync<NotFoundException>(() => _productService.CreateProductAsync(request));
    }

    [Fact]
    public async Task CreateProductAsync_ShouldThrow_WhenCategoryNotExists()
    {
        var user = new User { Id = Guid.NewGuid(), Username = "test" };
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        var request = new ProductRequest { OwnerId = user.Id, Category = "NonExisting", Title = "Test", Price = 10 };

        await Assert.ThrowsAsync<NotFoundException>(() => _productService.CreateProductAsync(request));
    }

    [Fact]
    public async Task ApproveProduct_ShouldUpdateApproval()
    {
        var product = new Product { Title = "Test", Approved = false, Category = "A", Price = 10, OwnerId = Guid.NewGuid() };
        _dbContext.Products.Add(product);
        await _dbContext.SaveChangesAsync();

        await _productService.ApproveProduct(product.Id, true);

        var updated = await _dbContext.Products.FindAsync(product.Id);
        Assert.True(updated.Approved);
    }

    [Fact]
    public async Task CreateProductByEmployeeAsync_ShouldCreateProductAndJob()
    {
        var userId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();

        _dbContext.Users.Add(new User { Id = userId });
        _dbContext.Categories.Add(new Category { Id = 1, Name = "Tech", Approved = true });
        await _dbContext.SaveChangesAsync();

        _mockUserService.Setup(u => u.GetIdByUsername("employee"))
                        .ReturnsAsync(employeeId);

        var request = new ProductRequest
        {
            OwnerId = userId,
            Title = "Phone",
            Price = 299.99m,
            Quantity = 10,
            Available = 10,
            Description = "Smartphone",
            Image = "img.png",
            Category = "Tech",
            Username = "employee"
        };

        var result = await _productService.CreateProductByEmployeeAsync(request);

        Assert.NotNull(result);
        Assert.False(result.Approved);
        _mockJobService.Verify(j => j.CreateJobAsync(It.Is<JobDto>(job => job.ProductId == result.Id && job.Type == JobType.Product && job.Operation == OperationType.Create)), Times.Once);
    }

    [Fact]
    public async Task UpdateProductAsync_ShouldUpdateFields()
    {
        var userId = Guid.NewGuid();
        var category = new Category { Name = "Gadgets" };
        _dbContext.Categories.Add(category);

        var product = new Product { Title = "Old", OwnerId = userId, Price = 100, Category = "Old", Approved = false };
        _dbContext.Products.Add(product);
        await _dbContext.SaveChangesAsync();

        var update = new ProductUpdate
        {
            Title = "New",
            Price = 120,
            Category = "Gadgets",
            //Approved = true
        };

        var result = await _productService.UpdateProductAsync(product.Id, update);

        Assert.Equal("New", result.Title);
        Assert.Equal(120, result.Price);
        Assert.Equal("Gadgets", result.Category);
    }

    [Fact]
    public async Task DeleteProductAsync_ShouldDelete_WhenExists()
    {
        var product = new Product { Title = "DeleteMe", Category = "X", Price = 10, OwnerId = Guid.NewGuid() };
        _dbContext.Products.Add(product);
        await _dbContext.SaveChangesAsync();

        var message = await _productService.DeleteProductAsync(product.Id);

        Assert.Equal("Product deleted successfully", message);
        Assert.Null(await _dbContext.Products.FindAsync(product.Id));
    }

    [Fact]
    public async Task DeleteProductByEmployeeAsync_ShouldCreateJob()
    {
        var userId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();

        _dbContext.Users.Add(new User { Id = userId });
        var product = new Product { Id = 123, Title = "Managed", Category = "X", Price = 10, OwnerId = userId };
        _dbContext.Products.Add(product);
        await _dbContext.SaveChangesAsync();

        _mockUserService.Setup(u => u.GetIdByUsername("employee"))
                        .ReturnsAsync(employeeId);

        var result = await _productService.DeleteProductByEmployeeAsync(product.Id, "employee");

        Assert.Equal("You can't delete a product", result);
        _mockJobService.Verify(j => j.CreateJobAsync(It.Is<JobDto>(job => job.ProductId == product.Id && job.Type == JobType.Product && job.Operation == OperationType.Delete)), Times.Once);
    }
}