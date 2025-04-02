using Api.Controllers;
using Api.DTOs;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Services.Interfaces;
using Shared.DTOs;
using Shared.Exceptions;
using Xunit;

public class CouponController_test
{
    private readonly Mock<ICouponService> _mockCouponService;
    private readonly CouponController _controller;

    public CouponController_test()
    {
        _mockCouponService = new Mock<ICouponService>();
        _controller = new CouponController(_mockCouponService.Object);
    }

    [Fact]
    public async Task GetAllCoupons_ShouldReturnOk_WithList()
    {
        // Arrange
        var coupons = new List<CouponDto>
        {
            new CouponDto { Id = 1, Code = "SUMMER", DiscountPercentage = 10 },
            new CouponDto { Id = 2, Code = "WINTER", DiscountPercentage = 20 }
        };
        _mockCouponService.Setup(s => s.GetAllCouponsAsync()).ReturnsAsync(coupons);

        // Act
        var result = await _controller.GetAllCoupons();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsAssignableFrom<IEnumerable<CouponDto>>(okResult.Value);
        Assert.Equal(2, response.Count());
    }

    [Fact]
    public async Task CreateCoupon_ShouldReturnCreated_WhenValid()
    {
        // Arrange
        var request = new CouponRequest { code = "NEWYEAR", discountPercentage = 15 };
        var dto = new CouponDto { Id = 1, Code = "NEWYEAR", DiscountPercentage = 15 };

        _mockCouponService.Setup(s => s.CreateCouponAsync(It.IsAny<CouponDto>())).ReturnsAsync(dto);

        // Act
        var result = await _controller.CreateCoupon(request);

        // Assert
        var createdResult = Assert.IsType<CreatedResult>(result);
        var coupon = Assert.IsType<CouponDto>(createdResult.Value);
        Assert.Equal("NEWYEAR", coupon.Code);
        Assert.Equal(15, coupon.DiscountPercentage);
    }

    [Fact]
    public async Task CreateCoupon_ShouldThrowBadRequest_WhenRequestIsNull()
    {
        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _controller.CreateCoupon(null));
    }

    [Fact]
    public async Task CreateCoupon_ShouldThrowBadRequest_WhenDiscountIs100OrMore()
    {
        // Arrange
        var request = new CouponRequest { code = "FREE", discountPercentage = 100 };

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _controller.CreateCoupon(request));
    }

    [Fact]
    public async Task DeleteCoupon_ShouldReturnOk_WhenCouponExists()
    {
        // Arrange
        var dto = new CouponDto { Id = 5, Code = "EXISTING", DiscountPercentage = 25 };

        _mockCouponService.Setup(s => s.GetCouponByCode("EXISTING")).ReturnsAsync(dto);
        _mockCouponService.Setup(s => s.DeleteCouponAsync(dto.Id)).ReturnsAsync(dto);

        // Act
        var result = await _controller.DeleteCoupon("EXISTING");

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<DefaultResponse>(okResult.Value);
        Assert.Equal("deleted coupon", response.message);
    }

    [Fact]
    public async Task DeleteCoupon_ShouldThrowNotFound_WhenCouponDoesNotExist()
    {
        // Arrange
        _mockCouponService.Setup(s => s.GetCouponByCode("MISSING")).ReturnsAsync((CouponDto?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _controller.DeleteCoupon("MISSING"));
    }

    [Fact]
    public async Task DeleteCoupon_ShouldThrowNotFound_WhenDeletionFails()
    {
        // Arrange
        var coupon = new CouponDto { Id = 9, Code = "FAIL", DiscountPercentage = 5 };

        _mockCouponService.Setup(s => s.GetCouponByCode("FAIL")).ReturnsAsync(coupon);
        _mockCouponService.Setup(s => s.DeleteCouponAsync(coupon.Id)).ReturnsAsync((CouponDto?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _controller.DeleteCoupon("FAIL"));
    }
}
