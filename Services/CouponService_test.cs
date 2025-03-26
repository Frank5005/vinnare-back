using Data;
using Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Services;
using Services.Utils;
using Shared.DTOs;
using Xunit;

public class CouponService_test
{
    private readonly VinnareDbContext _dbContext;
    private readonly Mock<ILogger<CouponService>> _mockLogger;
    private readonly CouponService _couponService;

    public CouponService_test()
    {
        _dbContext = TestDbContextFactory.Create();
        _dbContext.Database.EnsureCreated();

        _mockLogger = new Mock<ILogger<CouponService>>();
        _couponService = new CouponService(_dbContext, _mockLogger.Object);
    }

    [Fact]
    public async Task GetAllCouponsAsync_ShouldReturnAllCoupons()
    {
        // Arrange
        _dbContext.Database.EnsureDeleted();
        _dbContext.Database.EnsureCreated();

        _dbContext.Coupons.AddRange(
            new Coupon { Code = "SAVE10", DiscountPercentage = 10 },
            new Coupon { Code = "SAVE20", DiscountPercentage = 20 }
        );
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _couponService.GetAllCouponsAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.Contains(result, c => c.Code == "SAVE10" && c.DiscountPercentage == 10);
        Assert.Contains(result, c => c.Code == "SAVE20" && c.DiscountPercentage == 20);
    }

    [Fact]
    public async Task GetCouponByCode_ShouldReturnCoupon_WhenExists()
    {
        // Arrange
        _dbContext.Database.EnsureDeleted();
        _dbContext.Database.EnsureCreated();

        _dbContext.Coupons.Add(new Coupon { Code = "DISCOUNT50", DiscountPercentage = 50 });
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _couponService.GetCouponByCode("DISCOUNT50");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("DISCOUNT50", result!.Code);
        Assert.Equal(50, result.DiscountPercentage);
    }

    [Fact]
    public async Task GetCouponByCode_ShouldReturnNull_WhenNotFound()
    {
        // Arrange
        _dbContext.Database.EnsureDeleted();
        _dbContext.Database.EnsureCreated();

        // Act
        var result = await _couponService.GetCouponByCode("INVALID");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task CreateCouponAsync_ShouldAddCouponToDatabase()
    {
        // Arrange
        _dbContext.Database.EnsureDeleted();
        _dbContext.Database.EnsureCreated();

        var dto = new CouponDto { Code = "NEWCODE", DiscountPercentage = 15 };

        // Act
        var result = await _couponService.CreateCouponAsync(dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("NEWCODE", result.Code);
        Assert.Equal(15, result.DiscountPercentage);

        var dbEntity = await _dbContext.Coupons.FirstOrDefaultAsync(c => c.Code == "NEWCODE");
        Assert.NotNull(dbEntity);
        Assert.Equal(15, dbEntity!.DiscountPercentage);
    }

    [Fact]
    public async Task DeleteCouponAsync_ShouldRemoveCoupon_WhenExists()
    {
        // Arrange
        _dbContext.Database.EnsureDeleted();
        _dbContext.Database.EnsureCreated();

        var coupon = new Coupon { Code = "DELETE_ME", DiscountPercentage = 5 };
        _dbContext.Coupons.Add(coupon);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _couponService.DeleteCouponAsync(coupon.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("DELETE_ME", result!.Code);

        var exists = await _dbContext.Coupons.FindAsync(coupon.Id);
        Assert.Null(exists);
    }

    [Fact]
    public async Task DeleteCouponAsync_ShouldReturnNull_WhenNotFound()
    {
        // Arrange
        _dbContext.Database.EnsureDeleted();
        _dbContext.Database.EnsureCreated();

        // Act
        var result = await _couponService.DeleteCouponAsync(999);

        // Assert
        Assert.Null(result);
    }
}
