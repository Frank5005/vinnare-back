using Data;
using Data.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using Services;
using Services.Utils;
using Shared.DTOs;
using Xunit;

public class WishListService_test
{
    private readonly VinnareDbContext _dbContext;
    private readonly Mock<ILogger<WishListService>> _mockLogger;
    private readonly WishListService _wishListService;

    public WishListService_test()
    {
        _dbContext = TestDbContextFactory.Create();
        _dbContext.Database.EnsureCreated();

        _mockLogger = new Mock<ILogger<WishListService>>();
        _wishListService = new WishListService(_dbContext, _mockLogger.Object);
    }

    [Fact]
    public async Task GetAllWishListsAsync_ShouldReturnProductIdsForUser()
    {
        // Arrange
        _dbContext.Database.EnsureDeleted();
        _dbContext.Database.EnsureCreated();

        var userId = Guid.NewGuid();
        _dbContext.WishLists.AddRange(
            new WishList { UserId = userId, ProductId = 1 },
            new WishList { UserId = userId, ProductId = 2 },
            new WishList { UserId = Guid.NewGuid(), ProductId = 3 } // another user
        );
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _wishListService.GetAllWishListsAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.Contains(1, result);
        Assert.Contains(2, result);
        Assert.DoesNotContain(3, result);
    }

    [Fact]
    public async Task GetWishListByProductId_ShouldReturnWishListDto_WhenExists()
    {
        // Arrange
        _dbContext.Database.EnsureDeleted();
        _dbContext.Database.EnsureCreated();

        var userId = Guid.NewGuid();
        _dbContext.WishLists.Add(new WishList { UserId = userId, ProductId = 5 });
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _wishListService.GetWishListByProductId(userId, 5);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userId, result!.UserId);
        Assert.Equal(5, result.ProductId);
    }

    [Fact]
    public async Task GetWishListByProductId_ShouldReturnNull_WhenNotFound()
    {
        // Arrange
        _dbContext.Database.EnsureDeleted();
        _dbContext.Database.EnsureCreated();

        var userId = Guid.NewGuid();

        // Act
        var result = await _wishListService.GetWishListByProductId(userId, 999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task CreateWishListAsync_ShouldCreateAndReturnWishListDto()
    {
        // Arrange
        _dbContext.Database.EnsureDeleted();
        _dbContext.Database.EnsureCreated();

        var request = new CreateWishListRequest
        {
            UserId = Guid.NewGuid(),
            ProductId = 42
        };
        //var userId = Guid.NewGuid();

        // Act
        var result = await _wishListService.CreateWishListAsync(request);

        // Assert
        Assert.NotNull(result);
        //Assert.Equal(request.UserId, result.UserId);
        Assert.Equal(request.ProductId, result.ProductId);

        // Also verify it's saved in DB
        var dbEntity = await _dbContext.WishLists.FindAsync(result.Id);
        Assert.NotNull(dbEntity);
    }

    [Fact]
    public async Task DeleteWishListById_ShouldDeleteAndReturnWishListDto_WhenFound()
    {
        // Arrange
        _dbContext.Database.EnsureDeleted();
        _dbContext.Database.EnsureCreated();

        var wishList = new WishList
        {
            UserId = Guid.NewGuid(),
            ProductId = 99
        };
        _dbContext.WishLists.Add(wishList);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _wishListService.DeleteWishListById(wishList.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(wishList.Id, result!.Id);

        var exists = await _dbContext.WishLists.FindAsync(wishList.Id);
        Assert.Null(exists);
    }

    [Fact]
    public async Task DeleteWishListById_ShouldReturnNull_WhenNotFound()
    {
        // Arrange
        _dbContext.Database.EnsureDeleted();
        _dbContext.Database.EnsureCreated();

        // Act
        var result = await _wishListService.DeleteWishListById(999);

        // Assert
        Assert.Null(result);
    }
}
