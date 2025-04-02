using System.Security.Claims;
using Api.Controllers;
using Api.DTOs;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Services.Interfaces;
using Shared.DTOs;
using Shared.Exceptions;
using Xunit;

public class CartController_test
{
    private readonly Mock<ICartService> _mockCartService;
    private readonly Mock<IUserService> _mockUserService;
    private readonly Mock<IProductService> _mockProductService;
    private readonly CartController _controller;
    private readonly string _username = "testuser";
    private readonly Guid _userId = Guid.NewGuid();

    public CartController_test()
    {
        _mockCartService = new Mock<ICartService>();
        _mockUserService = new Mock<IUserService>();
        _mockProductService = new Mock<IProductService>();

        _controller = new CartController(
            _mockCartService.Object,
            _mockUserService.Object,
            _mockProductService.Object
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
    public async Task GetFullCartById_ShouldReturnOk_WhenCartExists()
    {
        // Arrange
        var fullCart = new List<CartItemDto>
    {
        new CartItemDto
        {
            ProductId = 1,
            Title = "Product 1",
            Price = 9.99M,
            Quantity = 2,
            Description = "Description",
            Category = "Books",
            Image = "image.jpg",
            CategoryId = 1,
            Available = 5
        }
    };

        _mockUserService.Setup(s => s.GetIdByUsername(_username)).ReturnsAsync(_userId);
        _mockCartService.Setup(s => s.GetFullCartByUserId(_userId)).ReturnsAsync(fullCart);

        // Act
        var result = await _controller.GetFullCartById();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedCart = Assert.IsAssignableFrom<IEnumerable<CartItemDto>>(okResult.Value);
        Assert.Single(returnedCart);
        Assert.Equal(2, returnedCart.First().Quantity);
        Assert.Equal("Product 1", returnedCart.First().Title);
    }

    [Fact]
    public async Task GetFullCartById_ShouldThrowNotFound_WhenCartIsNull()
    {
        _mockUserService.Setup(s => s.GetIdByUsername(_username)).ReturnsAsync(_userId);
        _mockCartService.Setup(s => s.GetFullCartByUserId(_userId)).ReturnsAsync((IEnumerable<CartItemDto>?)null);

        // Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _controller.GetFullCartById());
    }


    [Fact]
    public async Task GetCartById_ShouldReturnOk_WhenCartExists()
    {
        // Arrange
        var cart = new List<CartDto>
        {
            new CartDto { Id = 1, UserId = _userId, ProductId = 5, Quantity = 2 }
        };

        _mockUserService.Setup(s => s.GetIdByUsername(_username)).ReturnsAsync(_userId);
        _mockCartService.Setup(s => s.GetCartByUserId(_userId)).ReturnsAsync(cart);

        // Act
        var result = await _controller.GetCartById();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsAssignableFrom<IEnumerable<CartDto>>(okResult.Value);
        Assert.Single(response);
    }

    [Fact]
    public async Task GetCartById_ShouldThrowNotFound_WhenCartIsNull()
    {
        _mockUserService.Setup(s => s.GetIdByUsername(_username)).ReturnsAsync(_userId);
        _mockCartService.Setup(s => s.GetCartByUserId(_userId)).ReturnsAsync((IEnumerable<CartDto>?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _controller.GetCartById());
    }

    [Fact]
    public async Task CreateCart_ShouldReturnCreated_WhenValid()
    {
        var request = new CreateCartRequest { productId = 10, quantity = 3 };
         var product = new ProductDto { Id = 2, Available = 10, Approved = true };
        var cartDto = new CartDto { Id = 1, UserId = _userId, ProductId = 10, Quantity = 3 };

        _mockUserService.Setup(s => s.GetIdByUsername(_username)).ReturnsAsync(_userId);
        _mockProductService.Setup(s => s.GetProductForCartWishByIdAsync(10)).ReturnsAsync(product);
        _mockCartService.Setup(s => s.CreateCartAsync(It.IsAny<CartDto>())).ReturnsAsync(cartDto);

        var result = await _controller.CreateCart(request);

        var createdResult = Assert.IsType<CreatedResult>(result);
        var createdCart = Assert.IsType<CartDto>(createdResult.Value);
        Assert.Equal(10, createdCart.ProductId);
    }

    [Fact]
    public async Task CreateCart_ShouldThrowGone_WhenProductUnavailable()
    {
        var request = new CreateCartRequest { productId = 10, quantity = 1 };
        var product = new ProductDto { Id = 2, Available = 0, Approved = true };

        _mockUserService.Setup(s => s.GetIdByUsername(_username)).ReturnsAsync(_userId);
        _mockProductService.Setup(s => s.GetProductForCartWishByIdAsync(10)).ReturnsAsync(product);

        await Assert.ThrowsAsync<GoneException>(() => _controller.CreateCart(request));
    }

    [Fact]
    public async Task UpdateCart_ShouldReturnOk_WhenValid()
    {
        var product = new ProductDetail
        {
            Id = 10,
            Title = "Producto de prueba",
            Price = 99.99m,
            Description = "Descripción de prueba",
            Category = "Test",
            Image = "test.jpg",
            Rate = 5,
            Quantity = 10,
            Available = 5
        };
        var cart = new CartDto { Id = 99, ProductId = 10, UserId = _userId, Quantity = 1 };

        _mockUserService.Setup(s => s.GetIdByUsername(_username)).ReturnsAsync(_userId);
        _mockCartService.Setup(s => s.GetCartByUserId_ProductId(_userId, 10)).ReturnsAsync(cart);
        _mockProductService.Setup(s => s.GetProductByIdAsync(10)).ReturnsAsync(product);
        _mockCartService.Setup(s => s.UpdateCartQuantity(cart.Id, It.IsAny<int>())).ReturnsAsync(cart);

        var result = await _controller.UpdateCart(10, 3);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<DefaultResponse>(okResult.Value);
        Assert.Equal("ammount modified from cart", response.message);
    }

    [Fact]
    public async Task UpdateCart_ShouldThrow_WhenCartItemNotFound()
    {
        _mockUserService.Setup(s => s.GetIdByUsername(_username)).ReturnsAsync(_userId);
        _mockCartService.Setup(s => s.GetCartByUserId_ProductId(_userId, 10)).ReturnsAsync((CartDto?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _controller.UpdateCart(10, 3));
    }

    [Fact]
    public async Task DeleteCart_ShouldReturnOk_WhenValid()
    {
        var cart = new CartDto { Id = 44, ProductId = 7, UserId = _userId, Quantity = 2 };

        _mockUserService.Setup(s => s.GetIdByUsername(_username)).ReturnsAsync(_userId);
        _mockCartService.Setup(s => s.GetCartByUserId_ProductId(_userId, 7)).ReturnsAsync(cart);
        _mockCartService.Setup(s => s.DeleteCartAsync(cart.Id)).ReturnsAsync(cart);

        var result = await _controller.DeleteCart(7);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<DefaultResponse>(okResult.Value);
        Assert.Equal("Item removed from cart", response.message);
    }

    [Fact]
    public async Task DeleteCart_ShouldThrowBadRequest_WhenDeleteFails()
    {
        var cart = new CartDto { Id = 44, ProductId = 7, UserId = _userId, Quantity = 2 };

        _mockUserService.Setup(s => s.GetIdByUsername(_username)).ReturnsAsync(_userId);
        _mockCartService.Setup(s => s.GetCartByUserId_ProductId(_userId, 7)).ReturnsAsync(cart);
        _mockCartService.Setup(s => s.DeleteCartAsync(cart.Id)).ReturnsAsync((CartDto?)null);

        await Assert.ThrowsAsync<BadRequestException>(() => _controller.DeleteCart(7));
    }
}
