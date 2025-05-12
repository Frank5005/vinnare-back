using Data;
using Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Services;
using Services.Utils;
using Shared.DTOs;
using Xunit;

public class PurchaseService_test
{
    private VinnareDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<VinnareDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new VinnareDbContext(options);

        // Seed sample data
        var userId = Guid.NewGuid();

        context.Users.Add(new Data.Entities.User
        {
            Id = userId,
            Name = "John Doe",
            Address = "123 Main St"
        });

        _ = context.Purchases.Add(new Data.Entities.Purchase
        {
            Id = 1,
            UserId = userId,
            UserName = "John Doe",
            Products = new List<int> { 1, 2 },
            Prices = new List<decimal> { 10.0m, 20.0m },
            Quantities = new List<int> { 1, 2 },
            TotalPrice = 50.0m,
            TotalPriceBeforeDiscount = 60.0m,
            Date = DateTime.UtcNow,
            PaymentStatus = "paid",
            Status = "confirmed",
            Address = "123 Main St"
        });

        context.SaveChanges();
        return context;
    }

    [Fact]
    public async Task GetAllUserPurchases_ReturnsPurchasesForUser()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var loggerMock = new Mock<ILogger<PurchaseService>>();
        var service = new PurchaseService(context, loggerMock.Object);
        var userId = await context.Users.Select(u => u.Id).FirstAsync();

        // Act
        var result = await service.GetAllUserPurchases(userId);

        // Assert
        var purchaseList = Assert.IsAssignableFrom<IEnumerable<PurchaseDto>>(result);
        Assert.Single(purchaseList);
        Assert.Equal(userId, purchaseList.First().UserId);
        Assert.Equal("John Doe", purchaseList.First().UserName);
        Assert.Equal("123 Main St", purchaseList.First().Address);
    }

    [Fact]
    public async Task GetAllUserPurchases_ReturnsEmptyList_WhenUserHasNoPurchases()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var loggerMock = new Mock<ILogger<PurchaseService>>();
        var service = new PurchaseService(context, loggerMock.Object);
        var unknownUserId = Guid.NewGuid();

        // Act
        var result = await service.GetAllUserPurchases(unknownUserId);

        // Assert
        Assert.Empty(result);
    }

}
