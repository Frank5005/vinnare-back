using Data;
using Data.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using Services.Builders;
using Services.Interfaces;
using Services.Utils;
using Shared.DTOs;
using Shared.Exceptions;
using Xunit;

public class CartPurchaseBuilder_test
{
    private readonly VinnareDbContext _dbContext;
    private readonly Mock<ICouponService> _mockCouponService;
    private readonly Mock<ILogger<CartPurchaseBuilder>> _mockLogger;
    private readonly CartPurchaseBuilder _builder;
    private readonly Guid _userId = Guid.NewGuid();

    public CartPurchaseBuilder_test()
    {
        _dbContext = TestDbContextFactory.Create();
        _mockCouponService = new Mock<ICouponService>();
        _mockLogger = new Mock<ILogger<CartPurchaseBuilder>>();

        _builder = new CartPurchaseBuilder(_dbContext, _userId, _mockCouponService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task LoadCartAsync_ShouldLoadCart_WhenCartIsNotEmpty()
    {
        var product = new Product { Id = 1, Title = "Item", Approved = true, Available = 10, Price = 10, Category = "X", OwnerId = Guid.NewGuid() };
        var cart = new Cart { UserId = _userId, ProductId = 1, Quantity = 2, Product = product };

        _dbContext.Products.Add(product);
        _dbContext.Carts.Add(cart);
        await _dbContext.SaveChangesAsync();

        var result = await _builder.LoadCartAsync();

        Assert.NotNull(result);
        Assert.IsAssignableFrom<ICartPurchaseBuilder>(result);
    }

    [Fact]
    public async Task LoadCartAsync_ShouldThrow_WhenCartIsEmpty()
    {
        await Assert.ThrowsAsync<NotFoundException>(() => _builder.LoadCartAsync());
    }
    [Fact]
    public async Task ValidateApproved_ShouldThrow_WhenProductNotApproved()
    {
        var product = new Product { Id = 2, Title = "Bad", Approved = false, Available = 5, Price = 5, Category = "Y", OwnerId = Guid.NewGuid() };
        var cart = new Cart { UserId = _userId, ProductId = 2, Quantity = 1, Product = product };

        _dbContext.Products.Add(product);
        _dbContext.Carts.Add(cart);
        await _dbContext.SaveChangesAsync();

        await _builder.LoadCartAsync();

        Assert.Throws<GoneException>(() => _builder.ValidateApproved());
    }

    [Fact]
    public async Task ValidateStock_ShouldThrow_WhenInsufficientStock()
    {
        var product = new Product { Id = 3, Title = "LowStock", Approved = true, Available = 1, Price = 5, Category = "Z", OwnerId = Guid.NewGuid() };
        var cart = new Cart { UserId = _userId, ProductId = 3, Quantity = 5, Product = product };

        _dbContext.Products.Add(product);
        _dbContext.Carts.Add(cart);
        await _dbContext.SaveChangesAsync();

        await _builder.LoadCartAsync();

        Assert.Throws<GoneException>(() => _builder.ValidateStock());
    }

    [Fact]
    public async Task CalcBasePrice_ShouldCalculateCorrectly()
    {
        var product = new Product { Id = 4, Title = "Test", Approved = true, Available = 10, Price = 20, Category = "X", OwnerId = Guid.NewGuid() };
        var cart = new Cart { UserId = _userId, ProductId = 4, Quantity = 2, Product = product };

        _dbContext.Products.Add(product);
        _dbContext.Carts.Add(cart);
        await _dbContext.SaveChangesAsync();

        await _builder.LoadCartAsync();
        _builder.ValidateApproved();
        _builder.ValidateStock();
        var result = _builder.CalcBasePrice();

        Assert.IsAssignableFrom<ICartPurchaseBuilder>(result);
    }

    [Fact]
    public async Task FindCoupon_ShouldApplyDiscount()
    {
        _mockCouponService.Setup(s => s.GetCouponByCode("DISCOUNT10")).ReturnsAsync(new CouponDto
        {
            Code = "DISCOUNT10",
            DiscountPercentage = 10
        });

        await _builder.FindCoupon("DISCOUNT10");
        var result = _builder.CalcFinalPrice().FormatOutput();

        Assert.NotNull(result);
        Assert.Equal("DISCOUNT10", result.coupon_applied.coupon_code);
    }

    [Fact]
    public async Task CalcFinalPrice_ShouldCalculateWithShipping()
    {
        var product = new Product { Id = 5, Title = "Test", Approved = true, Available = 10, Price = 20, Category = "Y", OwnerId = Guid.NewGuid() };
        var cart = new Cart { UserId = _userId, ProductId = 5, Quantity = 1, Product = product };

        _dbContext.Products.Add(product);
        _dbContext.Carts.Add(cart);
        await _dbContext.SaveChangesAsync();

        await _builder.LoadCartAsync();
        _builder.ValidateApproved().ValidateStock().CalcBasePrice();
        var result = _builder.CalcFinalPrice().FormatOutput();

        Assert.NotNull(result);
        Assert.Equal(30, result.final_total); // 20 + 10 shipping
    }

    [Fact]
    public async Task DecrementStock_ShouldReduceProductStock()
    {
        var product = new Product { Id = 10, Title = "Stocky", Approved = true, Available = 10, Price = 5, Category = "Test", OwnerId = Guid.NewGuid() };
        var cart = new Cart { UserId = _userId, ProductId = 10, Quantity = 3, Product = product };

        _dbContext.Products.Add(product);
        _dbContext.Carts.Add(cart);
        await _dbContext.SaveChangesAsync();

        await _builder.LoadCartAsync();
        _builder.ValidateApproved().ValidateStock();

        _builder.DecrementStock();
        Assert.Equal(7, product.Available);
    }

    [Fact]
    public async Task CreatePurchase_ShouldAddPurchaseEntity()
    {
        var product = new Product { Id = 11, Title = "Purchasable", Approved = true, Available = 5, Price = 12.50m, Category = "Electronics", OwnerId = Guid.NewGuid() };
        var cart = new Cart { UserId = _userId, ProductId = 11, Quantity = 2, Product = product };

        _dbContext.Products.Add(product);
        _dbContext.Carts.Add(cart);
        await _dbContext.SaveChangesAsync();

        await _builder.LoadCartAsync();
        _builder.ValidateApproved()
               .ValidateStock()
               .CalcBasePrice();

        await _builder.FindCoupon(null);

        _builder.CalcFinalPrice()
               .CreatePurchase();

        await _builder.PersistAllChangesAsync();
        var purchase = _dbContext.Purchases.FirstOrDefault(p => p.UserId == _userId);
        Assert.NotNull(purchase);
        Assert.Equal(1, purchase.Products.Count);
        Assert.Equal(35m, purchase.TotalPrice);//it's 35 instead of 25 because of shipping
    }

    [Fact]
    public async Task ClearCart_ShouldRemoveAllUserCartItems()
    {
        var product = new Product { Id = 12, Title = "ToDelete", Approved = true, Available = 3, Price = 3, Category = "Temp", OwnerId = Guid.NewGuid() };
        var cart = new Cart { UserId = _userId, ProductId = 12, Quantity = 1, Product = product };

        _dbContext.Products.Add(product);
        _dbContext.Carts.Add(cart);
        await _dbContext.SaveChangesAsync();

        await _builder.LoadCartAsync();
        _builder.ClearCart();
        await _builder.PersistAllChangesAsync();

        var remaining = _dbContext.Carts.Where(c => c.UserId == _userId).ToList();
        Assert.Empty(remaining);
    }

    [Fact]
    public async Task RollbackTransaction_ShouldThrow_IfNotStarted()
    {
        await Assert.ThrowsAsync<BadRequestException>(() => _builder.RollbackTransactionAsync());
    }
    [Fact]
    public async Task CommitTransaction_ShouldThrow_IfNotStarted()
    {
        await Assert.ThrowsAsync<BadRequestException>(() => _builder.CommitTransactionAsync());
    }

}