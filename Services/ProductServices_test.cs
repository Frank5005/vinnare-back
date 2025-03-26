using Data;
using Data.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using Services;
using Services.Interfaces;
using Services.Utils;
using Xunit;

public class ProductService_test
{
    private readonly VinnareDbContext _dbContext;
    private readonly Mock<ILogger<ProductService>> _mockLogger;
    private readonly ProductService _productService;
    private readonly Mock<IJobService> _mockJobService;
    private readonly Mock<IUserService> _mockUserService;
    private readonly Mock<IReviewService> _mockReviewService;

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
    public async Task ApproveProduct_ShouldSetApproved_WhenProductExists()
    {
        // Arrange
        var product = new Product
        {
            Id = 1,
            Approved = false,
            Category = "TestCategory",
            OwnerId = Guid.NewGuid(),
            Title = "Test Product",
            Price = 9.99M
        };
        _dbContext.Products.Add(product);
        _dbContext.SaveChanges();

        // Act
        await _productService.ApproveProduct(1, true);
        var updatedProduct = await _dbContext.Products.FindAsync(1);

        // Assert
        Assert.NotNull(updatedProduct);
        Assert.True(updatedProduct.Approved);
    }

    [Fact]
    public async Task ApproveProduct_ShouldThrowException_WhenProductDoesNotExist()
    {
        // Act & Assert
        await Assert.ThrowsAsync<NullReferenceException>(() => _productService.ApproveProduct(999, true));
    }
}
