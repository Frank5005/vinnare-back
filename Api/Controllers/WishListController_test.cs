using System.Security.Claims;
using Api.Controllers;
using Api.DTOs;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Services.Interfaces;
using Shared.DTOs;
using Shared.Exceptions;
using Xunit;

public class WishListController_test
{
    private readonly Mock<IWishListService> _mockWishListService;
    private readonly Mock<IUserService> _mockUserService;
    private readonly Mock<IProductService> _mockProductService;
    private readonly WishListController _controller;

    public WishListController_test()
    {
        _mockWishListService = new Mock<IWishListService>();
        _mockUserService = new Mock<IUserService>();
        _mockProductService = new Mock<IProductService>();

        _controller = new WishListController(
            _mockWishListService.Object,
            _mockUserService.Object,
            _mockProductService.Object
        );
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

    /*
    [Fact]
    public async Task GetAllWishLists_ShouldReturnOk_WithWishList()
    {
        // Arrange
        var username = "testuser@example.com";
        var userId = Guid.NewGuid();
        SetUser(username);

        _mockUserService.Setup(s => s.GetIdByEmail(username)).ReturnsAsync(userId);
        _mockWishListService.Setup(s => s.GetAllWishListsAsync(userId)).ReturnsAsync(new List<int> { 1, 2, 3 });

        // Act
        var result = await _controller.GetAllWishLists();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<GetWishListResponse>(okResult.Value);
        Assert.Equal(userId, response.UserId);
        Assert.Equal(3, response.Products.Count());
    }
    */

    [Fact]
    public async Task CreateWishList_ShouldReturnOk_WhenValid()
    {
        // Arrange
        var username = "shopper1";
        var userId = Guid.NewGuid();
        SetUser(username);

        var request = new CreateWishListRequest
        {
            //UserId = userId,
            ProductId = 123
        };

        _mockUserService.Setup(s => s.GetIdByEmail(username)).ReturnsAsync(userId);
        _mockProductService.Setup(p => p.GetProductForCartWishByIdAsync(request.ProductId))
            .ReturnsAsync(new ProductDto { Id = 123, Approved = true });

        // Act
        var result = await _controller.CreateWishList(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<DefaultResponse>(okResult.Value);
        Assert.Equal("Successfully added", response.message);
    }

    /*
    [Fact]
    public async Task CreateWishList_ShouldThrowUnauthorized_WhenUserMismatch()
    {
        // Arrange
        var username = "user1";
        var tokenUserId = Guid.NewGuid();
        SetUser(username);

        var request = new CreateWishListRequest
        {
            //UserId = Guid.NewGuid(), // different user
            ProductId = 99
        };

        _mockUserService.Setup(s => s.GetIdByEmail(username)).ReturnsAsync(tokenUserId);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedException>(() => _controller.CreateWishList(request));
    }
    */

    [Fact]
    public async Task DeleteWishList_ShouldReturnOk_WhenValid()
    {
        // Arrange
        var username = "shopper2";
        var userId = Guid.NewGuid();
        var productId = 55;
        var wishListDto = new WishListDto { Id = 1, UserId = userId, ProductId = productId };

        SetUser(username);

        _mockUserService.Setup(s => s.GetIdByEmail(username)).ReturnsAsync(userId);
        _mockWishListService.Setup(s => s.GetWishListByProductId(userId, productId)).ReturnsAsync(wishListDto);
        _mockWishListService.Setup(s => s.DeleteWishListById(wishListDto.Id)).ReturnsAsync(wishListDto);

        // Act
        var result = await _controller.DeleteWishList(productId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<DefaultResponse>(okResult.Value);
        Assert.Equal("Successfully deleted", response.message);
    }

    [Fact]
    public async Task DeleteWishList_ShouldThrowNotFound_WhenWishListNotFound()
    {
        // Arrange
        var username = "user404";
        var userId = Guid.NewGuid();
        var productId = 999;

        SetUser(username);

        _mockUserService.Setup(s => s.GetIdByEmail(username)).ReturnsAsync(userId);
        _mockWishListService.Setup(s => s.GetWishListByProductId(userId, productId)).ReturnsAsync((WishListDto?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _controller.DeleteWishList(productId));
    }
}
