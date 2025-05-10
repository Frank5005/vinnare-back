using Data;
using Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Services.Builders;
using Services.Interfaces;
using Services.Utils;
using Shared.DTOs;
using Shared.Exceptions;
using Xunit;

public class CartPurchaseBuilder_test
{
    private VinnareDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<VinnareDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new VinnareDbContext(options);
    }

    private Cart CreateCartItem(Guid userId, bool approved = true, int quantity = 1, int available = 5, decimal price = 10m, string category = "Books")
    {
        return new Cart
        {
            UserId = userId,
            Quantity = quantity,
            Product = new Product
            {
                Id = 1,
                Approved = approved,
                Available = available,
                Price = price,
                Title = "Sample Product",
                Category = category
            },
            ProductId = 1
        };
    }

    [Fact]
    public async Task LoadCartAsync_ThrowsNotFoundException_WhenCartEmpty()
    {
        // Arrange
        var dbContext = CreateInMemoryDbContext();
        var couponService = new Mock<ICouponService>();
        var builder = new CartPurchaseBuilder(dbContext, Guid.NewGuid(), couponService.Object, new NullLogger<CartPurchaseBuilder>());

        // Act & Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(() => builder.LoadCartAsync());
        Assert.Equal("Cart is empty; cannot proceed.", ex.Message);
    }

    [Fact]
    public async Task ValidateApproved_ThrowsException_WhenUnapprovedProduct()
    {
        var dbContext = CreateInMemoryDbContext();
        var userId = Guid.NewGuid();
        var cart = CreateCartItem(userId, approved: false);

        dbContext.Carts.Add(cart);
        await dbContext.SaveChangesAsync();

        var couponService = new Mock<ICouponService>();
        var builder = new CartPurchaseBuilder(dbContext, userId, couponService.Object, new NullLogger<CartPurchaseBuilder>());

        await builder.LoadCartAsync();
        var ex = Assert.Throws<GoneException>(() => builder.ValidateApproved());
        Assert.Equal("Product 1 is not available for purchase anymore.", ex.Message);
    }

    [Fact]
    public async Task ValidateStock_ThrowsException_WhenNotEnoughStock()
    {
        var dbContext = CreateInMemoryDbContext();
        var userId = Guid.NewGuid();
        var cart = CreateCartItem(userId, quantity: 10, available: 5);

        dbContext.Carts.Add(cart);
        await dbContext.SaveChangesAsync();

        var couponService = new Mock<ICouponService>();
        var builder = new CartPurchaseBuilder(dbContext, userId, couponService.Object, new NullLogger<CartPurchaseBuilder>());

        await builder.LoadCartAsync().ConfigureAwait(false);
        builder.ValidateApproved();
        var ex = Assert.Throws<GoneException>(() => builder.ValidateStock());
        Assert.Equal("Product 1 is out of stock or not enough stock.", ex.Message);
    }

    [Fact]
    public async Task CalcFinalPrice_WithCoupon_AppliesDiscountCorrectly()
    {
        var dbContext = CreateInMemoryDbContext();
        var userId = Guid.NewGuid();
        var cart = CreateCartItem(userId, price: 100m);
        dbContext.Carts.Add(cart);
        await dbContext.SaveChangesAsync();

        var couponService = new Mock<ICouponService>();
        couponService.Setup(x => x.GetCouponByCode("DISCOUNT10"))
            .ReturnsAsync(new CouponDto { Code = "DISCOUNT10", DiscountPercentage = 10 });

        var builder = new CartPurchaseBuilder(dbContext, userId, couponService.Object, new NullLogger<CartPurchaseBuilder>());

        await builder.LoadCartAsync();
        builder.ValidateApproved().ValidateStock().CalcBasePrice();
        await builder.FindCoupon("DISCOUNT10");
        builder.CalcFinalPrice();
        var result = builder.FormatOutput();

        Assert.NotNull(result);
        Assert.Equal(100m, result.total_before_discount);
        Assert.Equal(90m, result.total_after_discount);
        Assert.Equal(100m, result.final_total); // 90 + 10 shipping
    }

    [Fact]
    public async Task DecrementStock_DecreasesProductAvailability()
    {
        var dbContext = CreateInMemoryDbContext();
        var userId = Guid.NewGuid();
        var cart = CreateCartItem(userId, quantity: 2, available: 10);
        dbContext.Carts.Add(cart);
        await dbContext.SaveChangesAsync();

        var couponService = new Mock<ICouponService>();
        var builder = new CartPurchaseBuilder(dbContext, userId, couponService.Object, new NullLogger<CartPurchaseBuilder>());

        await builder.LoadCartAsync();
        builder.ValidateApproved().ValidateStock().DecrementStock();
        var updatedProduct = dbContext.Carts.Include(c => c.Product).First().Product;

        Assert.Equal(8, updatedProduct.Available);
    }

    [Fact]
    public async Task CreatePurchase_AddsPurchaseToDatabase()
    {
        var dbContext = CreateInMemoryDbContext();
        var userId = Guid.NewGuid();
        var cart = CreateCartItem(userId);
        dbContext.Carts.Add(cart);
        await dbContext.SaveChangesAsync();

        var couponService = new Mock<ICouponService>();
        var builder = new CartPurchaseBuilder(dbContext, userId, couponService.Object, new NullLogger<CartPurchaseBuilder>());

        await builder.LoadCartAsync();
        builder.ValidateApproved().ValidateStock().CalcBasePrice().CalcFinalPrice().CreatePurchase();
        await builder.PersistAllChangesAsync();

        var purchase = await dbContext.Purchases.FirstOrDefaultAsync();
        Assert.NotNull(purchase);
        Assert.Equal(userId, purchase.UserId);
    }
}