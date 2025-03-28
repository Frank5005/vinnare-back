using Data;
using Data.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using Services;
using Services.Utils;
using Xunit;

public class PurchaseService_test
{
    private readonly VinnareDbContext _dbContext;
    private readonly Mock<ILogger<PurchaseService>> _mockLogger;
    private readonly PurchaseService _service;

    public PurchaseService_test()
    {
        _dbContext = TestDbContextFactory.Create();
        _mockLogger = new Mock<ILogger<PurchaseService>>();
        _service = new PurchaseService(_dbContext, _mockLogger.Object);
    }

    [Fact]
    public async Task GetAllUserPurchases_ShouldReturnUserPurchasesWithCorrectData()
    {
        // Arrange
        var userId = Guid.NewGuid();

        var purchase1 = new Purchase
        {
            UserId = userId,
            Products = new List<int> { 1, 2 },
            Prices = new List<decimal> { 10.99M, 5.50M },
            Quantities = new List<int> { 1, 2 },
            TotalPrice = 21.99M,
            TotalPriceBeforeDiscount = 24.00M,
            CouponCode = "SAVE10"
        };

        var purchase2 = new Purchase
        {
            UserId = userId,
            Products = new List<int> { 3 },
            Prices = new List<decimal> { 15.00M },
            Quantities = new List<int> { 1 },
            TotalPrice = 15.00M,
            TotalPriceBeforeDiscount = 15.00M
        };

        _dbContext.Purchases.AddRange(purchase1, purchase2);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.GetAllUserPurchases(userId);

        // Assert
        Assert.NotNull(result);
        var list = result.ToList();
        Assert.Equal(2, list.Count);
        Assert.Equal(2, list[0].Products.Count);
        Assert.Equal("SAVE10", _dbContext.Purchases.First().CouponCode);
    }

}
