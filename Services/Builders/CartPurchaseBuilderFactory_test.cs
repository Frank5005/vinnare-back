using Microsoft.Extensions.Logging;
using Moq;
using Services.Builders;
using Services.Interfaces;
using Services.Utils;
using Xunit;

public class CartPurchaseBuilderFactory_test
{
    [Fact]
    public void Create_ShouldReturnCartPurchaseBuilder_WithCorrectDependencies()
    {
        // Arrange
        var dbContext = TestDbContextFactory.Create();
        var mockCouponService = new Mock<ICouponService>();
        var mockLogger = new Mock<ILogger<CartPurchaseBuilder>>();
        var factory = new CartPurchaseBuilderFactory(dbContext, mockCouponService.Object, mockLogger.Object);

        var userId = Guid.NewGuid();

        // Act
        var builder = factory.Create(userId);

        // Assert
        Assert.NotNull(builder);
        Assert.IsAssignableFrom<ICartPurchaseBuilder>(builder);
    }
}
