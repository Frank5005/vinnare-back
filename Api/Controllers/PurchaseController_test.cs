using System.Security.Claims;
using Api.Controllers;
using Api.DTOs;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Services.Builders;
using Services.Interfaces;
using Shared.DTOs;
using Shared.Exceptions;
using Xunit;

public class PurchaseController_test
{
    private readonly Mock<ICartPurchaseBuilderFactory> _mockBuilderFactory;
    private readonly Mock<IPurchaseService> _mockPurchaseService;
    private readonly Mock<IUserService> _mockUserService;
    private readonly Mock<ILogger<PurchaseController>> _mockLogger;
    private readonly PurchaseController _controller;
    private readonly string _username = "testuser@example.com";
    private readonly Guid _userId = Guid.NewGuid();

    public PurchaseController_test()
    {
        _mockBuilderFactory = new Mock<ICartPurchaseBuilderFactory>();
        _mockPurchaseService = new Mock<IPurchaseService>();
        _mockUserService = new Mock<IUserService>();
        _mockLogger = new Mock<ILogger<PurchaseController>>();

        _controller = new PurchaseController(
            _mockPurchaseService.Object,
            _mockUserService.Object,
            _mockLogger.Object,
            _mockBuilderFactory.Object
        );

        SetUser(_username);
    }

    private void SetUser(string username)
    {
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, username) };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };
    }

    [Fact]
    public async Task GetAllUserPurchases_ShouldReturnPurchases()
    {
        // Arrange
        var purchases = new List<PurchaseDto>
        {
            new PurchaseDto { Id = 1, UserId = _userId, Products = new List<int> { 1, 2 }, Date = DateTime.UtcNow }
        };

        _mockUserService.Setup(u => u.GetIdByEmail(_username)).ReturnsAsync(_userId);
        _mockPurchaseService.Setup(p => p.GetAllUserPurchases(_userId)).ReturnsAsync(purchases);

        // Act
        var result = await _controller.GetAllUserPurchases();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returned = Assert.IsAssignableFrom<IEnumerable<PurchaseDto>>(okResult.Value);
        Assert.Single(returned);
    }


    [Fact]
    public async Task Preview_ShouldReturnPurchaseResponse()
    {
        // Arrange
        var builder = new Mock<ICartPurchaseBuilder>();
        var finalResult = new PurchaseResponse
        {
            user_id = _userId,
            final_total = 123,
            total_before_discount = 100,
            total_after_discount = 110,
            shopping_cart = new[] { 1 },
            coupon_applied = new CouponData { coupon_code = "ABC", discount_percentage = 10 },
            shipping_cost = 13
        };

        _mockUserService.Setup(u => u.GetIdByEmail(_username)).ReturnsAsync(_userId);
        _mockBuilderFactory.Setup(f => f.Create(_userId)).Returns(builder.Object);

        // Fluent builder chain
        builder.Setup(b => b.LoadCartAsync()).ReturnsAsync(builder.Object);
        builder.Setup(b => b.ValidateApproved()).Returns(builder.Object);
        builder.Setup(b => b.ValidateStock()).Returns(builder.Object);
        builder.Setup(b => b.CalcBasePrice()).Returns(builder.Object);
        builder.Setup(b => b.FindCoupon(It.IsAny<string>())).ReturnsAsync(builder.Object);
        builder.Setup(b => b.CalcFinalPrice()).Returns(builder.Object);
        builder.Setup(b => b.FormatOutput()).Returns(finalResult);

        // Act
        var result = await _controller.Preview(new PurchaseRequest { coupon_code = "ABC" });

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<PurchaseResponse>(okResult.Value);
        Assert.Equal(123, response.final_total);
        Assert.Equal("ABC", response.coupon_applied.coupon_code);
    }

    [Fact]
    public async Task Buy_ShouldReturnCreated_WhenBuilderCompletes()
    {
        // Arrange
        var builder = new Mock<ICartPurchaseBuilder>();
        var response = new PurchaseResponse { final_total = 99, user_id = _userId, shopping_cart = new[] { 1, 2 } };

        _mockUserService.Setup(u => u.GetIdByEmail(_username)).ReturnsAsync(_userId);
        _mockBuilderFactory.Setup(f => f.Create(_userId)).Returns(builder.Object);

        // Builder chain
        builder.Setup(b => b.LoadCartAsync()).ReturnsAsync(builder.Object);
        builder.Setup(b => b.ValidateApproved()).Returns(builder.Object);
        builder.Setup(b => b.ValidateStock()).Returns(builder.Object);
        builder.Setup(b => b.CalcBasePrice()).Returns(builder.Object);
        builder.Setup(b => b.FindCoupon(It.IsAny<string>())).ReturnsAsync(builder.Object);
        builder.Setup(b => b.CalcFinalPrice()).Returns(builder.Object);
        builder.Setup(b => b.BeginTransactionAsync()).ReturnsAsync(builder.Object);
        builder.Setup(b => b.DecrementStock()).Returns(builder.Object);
        builder.Setup(b => b.CreatePurchase()).Returns(builder.Object);
        builder.Setup(b => b.ClearCart()).Returns(builder.Object);
        builder.Setup(b => b.PersistAllChangesAsync()).ReturnsAsync(builder.Object);
        builder.Setup(b => b.CommitTransactionAsync()).ReturnsAsync(builder.Object);
        builder.Setup(b => b.AddMetricsData()).Returns(builder.Object);
        builder.Setup(b => b.FormatOutput()).Returns(response);

        // Act
        var result = await _controller.Buy(new PurchaseRequest { coupon_code = null });

        // Assert
        var created = Assert.IsType<CreatedResult>(result);
        var payload = Assert.IsType<PurchaseResponse>(created.Value);
        Assert.Equal(99, payload.final_total);
        Assert.Equal(_userId, payload.user_id);
    }

    [Fact]
    public async Task Buy_ShouldRollbackAndThrow_WhenBuilderThrows()
    {
        var builder = new Mock<ICartPurchaseBuilder>();

        _mockUserService.Setup(u => u.GetIdByEmail(_username)).ReturnsAsync(_userId);
        _mockBuilderFactory.Setup(f => f.Create(_userId)).Returns(builder.Object);

        builder.Setup(b => b.LoadCartAsync()).ReturnsAsync(builder.Object);
        builder.Setup(b => b.ValidateApproved()).Returns(builder.Object);
        builder.Setup(b => b.ValidateStock()).Returns(builder.Object);
        builder.Setup(b => b.CalcBasePrice()).Returns(builder.Object);
        builder.Setup(b => b.FindCoupon(It.IsAny<string>())).ReturnsAsync(builder.Object);
        builder.Setup(b => b.CalcFinalPrice()).Returns(builder.Object);
        builder.Setup(b => b.BeginTransactionAsync()).ReturnsAsync(builder.Object);
        builder.Setup(b => b.DecrementStock()).Returns(builder.Object);
        builder.Setup(b => b.CreatePurchase()).Throws(new Exception("DB crashed"));
        builder.Setup(b => b.RollbackTransactionAsync()).ReturnsAsync(builder.Object);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<BadRequestException>(() => _controller.Buy(new PurchaseRequest { coupon_code = null }));
        Assert.Equal("something failed. Please try again", ex.Message);
    }
}

