using System.Security.Claims;
using Api.Controllers;
using Api.Utils;
using Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Services.Interfaces;
using Shared.DTOs;
using Xunit;

public class ProductController_test
{
    private readonly Mock<IProductService> _mockProductService;
    private readonly Mock<ICacheHelper> _mockCacheHelper;
    private readonly ProductController _controller;

    public ProductController_test()
    {
        _mockProductService = new Mock<IProductService>();
        _mockCacheHelper = new Mock<ICacheHelper>();
        _controller = new ProductController(_mockProductService.Object, _mockCacheHelper.Object);
    }

    private void SetUserWithRole(string role, string username = "testuser")
    {
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Role, role),
                    new Claim(ClaimTypes.NameIdentifier, username)
                }, "mock"))
            }
        };
    }

    [Fact]
    public async Task GetAllProducts_ShouldReturnOk()
    {
        _mockProductService.Setup(s => s.GetAllProductsAsync()).ReturnsAsync(new List<ProductDto>());

        SetUserWithRole("Admin");
        var result = await _controller.GetAllProducts();

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.IsAssignableFrom<IEnumerable<ProductDto>>(ok.Value);
    }

    [Fact]
    public async Task GetAvailableProductsPage_ShouldReturnOk()
    {
        _mockProductService.Setup(s => s.GetAvailableProductsPageAsync()).ReturnsAsync(new List<ProductViewPage>());

        var result = await _controller.GetAvailableProductsPage();

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.IsAssignableFrom<IEnumerable<ProductViewPage>>(ok.Value);
    }

    [Fact]
    public async Task GetProductsByCategory_ShouldReturnOk()
    {
        _mockProductService.Setup(s => s.GetProductsByCategoryAsync(1)).ReturnsAsync(new List<ProductViewPage>());

        var result = await _controller.GetProductsByCategory(1);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.IsAssignableFrom<IEnumerable<ProductViewPage>>(ok.Value);
    }

    [Fact]
    public async Task GetProductById_ShouldReturnOk_WhenExists()
    {
        _mockProductService.Setup(s => s.GetProductByIdAsync(1)).ReturnsAsync(new ProductDetail { Id = 1 });

        var result = await _controller.GetProductById(1);

        var ok = Assert.IsType<OkObjectResult>(result);
        var product = Assert.IsType<ProductDetail>(ok.Value);
        Assert.Equal(1, product.Id);
    }

    [Fact]
    public async Task GetProductById_ShouldReturnNotFound_WhenMissing()
    {
        _mockProductService.Setup(s => s.GetProductByIdAsync(99)).ReturnsAsync((ProductDetail?)null);

        var result = await _controller.GetProductById(99);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task CreateProduct_ShouldReturnCreated_WhenAdmin()
    {
        SetUserWithRole("Admin");
        var request = new ProductRequest { Title = "Book", Price = 10 };
        var created = new Product { Id = 1 };

        _mockProductService.Setup(s => s.CreateProductAsync(request)).ReturnsAsync(created);

        var result = await _controller.CreateProduct(request);

        var ok = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ProductResponse>(ok.Value);
        Assert.Equal("Product created successfully", response.message);
    }

    [Fact]
    public async Task CreateProduct_ShouldReturnJob_WhenSeller()
    {
        SetUserWithRole("Seller");
        var request = new ProductRequest { Title = "Book", Price = 10 };
        var created = new Product { Id = 2 };

        _mockProductService.Setup(s => s.CreateProductByEmployeeAsync(request)).ReturnsAsync(created);

        var result = await _controller.CreateProduct(request);

        var ok = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ProductResponse>(ok.Value);
        Assert.Equal("Waiting to approve", response.message);
    }

    [Fact]
    public async Task UpdateProduct_ShouldReturnOk_WhenUpdated()
    {
        SetUserWithRole("Admin");
        var updated = new Product { Id = 1 };

        _mockProductService.Setup(s => s.UpdateProductAsync(1, It.IsAny<ProductUpdate>())).ReturnsAsync(updated);

        var result = await _controller.UpdateProduct(1, new ProductUpdate());

        var ok = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ProductResponse>(ok.Value);
        Assert.Equal("Product updated successfully", response.message);
    }

    [Fact]
    public async Task DeleteProduct_ShouldReturnOk_WhenAdmin()
    {
        SetUserWithRole("Admin");
        _mockProductService.Setup(s => s.DeleteProductAsync(1)).ReturnsAsync("Product deleted");

        var result = await _controller.DeleteProduct(1, "adminuser");

        var ok = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ProductDelete>(ok.Value);
        Assert.Equal("Product deleted", response.message);
    }

    [Fact]
    public async Task DeleteProduct_ShouldReturnOk_WhenSeller()
    {
        SetUserWithRole("Seller");
        _mockProductService.Setup(s => s.DeleteProductByEmployeeAsync(1, "selleruser"))
            .ReturnsAsync("Job created for deletion");

        var result = await _controller.DeleteProduct(1, "selleruser");

        var ok = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ProductDelete>(ok.Value);
        Assert.Equal("Job created for deletion", response.message);
    }
}
