using Data;
using Data.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using Services;
using Services.Utils;
using Shared.DTOs;
using Xunit;

public class CartService_test
{
    private readonly VinnareDbContext _dbContext;
    private readonly Mock<ILogger<CartService>> _mockLogger;
    private readonly CartService _cartService;

    public CartService_test()
    {
        _dbContext = TestDbContextFactory.Create();
        _mockLogger = new Mock<ILogger<CartService>>();
        _cartService = new CartService(_dbContext, _mockLogger.Object);
    }


    [Fact]
    public async Task GetFullCartByUserId_ShouldReturnDetailedCartItems()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var product = new Product
        {
            Id = 1,
            Title = "Test Product",
            Price = 19.99M,
            Description = "A great item",
            Category = "Electronics",
            Image = "image.jpg",
            CategoryId = 3,
            Available = 10,
            OwnerId = Guid.NewGuid()
        };

        _dbContext.Products.Add(product);
        _dbContext.Carts.Add(new Cart
        {
            UserId = userId,
            ProductId = product.Id,
            Quantity = 2,
            Product = product
        });
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _cartService.GetFullCartByUserId(userId);

        // Assert
        Assert.NotNull(result);
        var item = result.First();
        Assert.Equal(1, item.ProductId);
        Assert.Equal("Test Product", item.Title);
        Assert.Equal(2, item.Quantity);
        Assert.Equal(19.99M, item.Price);
        Assert.Equal(10, item.Available);
    }

    [Fact]
    public async Task GetCartByUserId_ShouldReturnUserCarts()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        _dbContext.Carts.AddRange(
            new Cart { UserId = userId, ProductId = 10, Quantity = 1 },
            new Cart { UserId = userId, ProductId = 20, Quantity = 2 },
            new Cart { UserId = otherUserId, ProductId = 30, Quantity = 3 }
        );
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _cartService.GetCartByUserId(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.All(result, c => Assert.Equal(userId, c.UserId));
    }

    [Fact]
    public async Task GetCartByUserId_ProductId_ShouldReturnSingleCart()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _dbContext.Carts.Add(new Cart { UserId = userId, ProductId = 99, Quantity = 5 });
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _cartService.GetCartByUserId_ProductId(userId, 99);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(99, result!.ProductId);
        Assert.Equal(userId, result.UserId);
        Assert.Equal(5, result.Quantity);
    }

    [Fact]
    public async Task GetCartByUserId_ProductId_ShouldReturnNull_IfNotExists()
    {
        // Arrange
        var result = await _cartService.GetCartByUserId_ProductId(Guid.NewGuid(), 999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task CreateCartAsync_ShouldAddCartAndReturnDto()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var dto = new CartDto
        {
            UserId = userId,
            ProductId = 101,
            Quantity = 2
        };

        // Act
        var result = await _cartService.CreateCartAsync(dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userId, result.UserId);
        Assert.Equal(101, result.ProductId);
        Assert.Equal(2, result.Quantity);

        var dbCart = await _dbContext.Carts.FindAsync(result.Id);
        Assert.NotNull(dbCart);
    }

    [Fact]
    public async Task UpdateCartQuantity_ShouldUpdate_WhenCartExists()
    {
        // Arrange
        var cart = new Cart { UserId = Guid.NewGuid(), ProductId = 777, Quantity = 1 };
        _dbContext.Carts.Add(cart);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _cartService.UpdateCartQuantity(cart.Id, 5);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(5, result!.Quantity);
    }

    [Fact]
    public async Task UpdateCartQuantity_ShouldReturnNull_WhenCartNotFound()
    {
        // Act
        var result = await _cartService.UpdateCartQuantity(999, 10);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteCartAsync_ShouldDeleteCartAndReturnDto()
    {
        // Arrange
        var cart = new Cart { UserId = Guid.NewGuid(), ProductId = 888, Quantity = 3 };
        _dbContext.Carts.Add(cart);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _cartService.DeleteCartAsync(cart.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(cart.Id, result!.Id);
        Assert.Null(await _dbContext.Carts.FindAsync(cart.Id));
    }

    [Fact]
    public async Task DeleteCartAsync_ShouldReturnNull_WhenNotFound()
    {
        // Act
        var result = await _cartService.DeleteCartAsync(404);

        // Assert
        Assert.Null(result);
    }
}
