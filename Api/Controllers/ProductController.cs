using System.Security.Claims;
using Api.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;
using Shared.DTOs;
using Shared.Exceptions;

namespace Api.Controllers
{
    [Route("api/product")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly ICacheHelper _cacheHelper;
        private readonly IProductService _productService;
        private const string ALL_PRODUCTS_CACHE_KEY = "all_products";
        private const string AVAILABLE_PRODUCTS_PAGE_CACHE_KEY = "available_products_page";

        public ProductController(IProductService productService, ICacheHelper cacheHelper)
        {
            _productService = productService;
            _cacheHelper = cacheHelper;
        }

        // GET: api/product/all
        [Authorize(Roles = "Admin, Seller")]
        [HttpGet("all")]
        public async Task<IActionResult> GetAllProducts()
        {
            if (_cacheHelper.TryGetValue<ProductDto>(ALL_PRODUCTS_CACHE_KEY, out var cachedProducts))
                return Ok(cachedProducts);

            var products = await _productService.GetAllProductsAsync();
            _cacheHelper.Set<ProductDto>(ALL_PRODUCTS_CACHE_KEY, products.ToList());
            return Ok(products);
        }

        // View Product User Story
        // GET: api/product
        /*
        [HttpGet]
        public async Task<IActionResult> GetAvailableProducts()
        {
            var products = await _productService.GetAvailableProductsAsync();
            return Ok(products);
        }
        */

        // Product Display Page User Story
        // GET: api/product/store
        [HttpGet("store")]
        public async Task<IActionResult> GetAvailableProductsPage()
        {
            if (_cacheHelper.TryGetValue<ProductViewPage>(AVAILABLE_PRODUCTS_PAGE_CACHE_KEY, out var cachedProducts))
                return Ok(cachedProducts);

            var products = await _productService.GetAvailableProductsPageAsync();
            _cacheHelper.Set<ProductViewPage>(AVAILABLE_PRODUCTS_PAGE_CACHE_KEY, products.ToList());

            return Ok(products);
        }

        // GET: apiproduct/store/{category}
        [HttpGet("store/{id:int}")]
        public async Task<IActionResult> GetProductsByCategory(int id)
        {
            var products = await _productService.GetProductsByCategoryAsync(id);
            return Ok(products);
        }

        // GET: api/product/search/{name}
        [HttpGet("search/{name}")]
        public async Task<IActionResult> SearchProductsByName(string name)
        {
            var products = await _productService.SearchProductsByNameAsync(name);
            return Ok(products);
        }

        // GET: api/product/{id}
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetProductById(int id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null) return NotFound();
            return Ok(product);
        }

        // POST: api/product/create
        [Authorize(Roles = "Admin, Seller")]
        [HttpPost("create")]
        public async Task<IActionResult> CreateProduct([FromBody] ProductRequest productDto)
        {
            if (productDto == null) throw new BadRequestException("Product data is required.");

            var tokenRole = User.FindFirst(ClaimTypes.Role)?.Value;
            if (tokenRole == "Admin")
            {
                var createdProduct = await _productService.CreateProductAsync(productDto);
                _cacheHelper.RemoveKeys(ALL_PRODUCTS_CACHE_KEY, AVAILABLE_PRODUCTS_PAGE_CACHE_KEY);
                return Ok(new ProductResponse { Id = createdProduct.Id, message = "Product created successfully" });
            }
            else
            {
                var createdProduct = await _productService.CreateProductByEmployeeAsync(productDto);
                _cacheHelper.RemoveKeys(ALL_PRODUCTS_CACHE_KEY, AVAILABLE_PRODUCTS_PAGE_CACHE_KEY);
                return Ok(new ProductResponse { Id = createdProduct.Id, message = "Waiting to approve" });
            }
        }

        // UPDATE: api/product/{id}
        [Authorize(Roles = "Admin, Seller")]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] ProductUpdate productDto)
        {
            if (productDto == null) throw new BadRequestException("Product data is required.");

            var updatedProduct = await _productService.UpdateProductAsync(id, productDto);
            if (updatedProduct == null) throw new NotFoundException("Product not found.");
            _cacheHelper.RemoveKeys(ALL_PRODUCTS_CACHE_KEY, AVAILABLE_PRODUCTS_PAGE_CACHE_KEY);
            return Ok(new ProductResponse { Id = updatedProduct.Id, message = "Product updated successfully" });
        }

        // DELETE: api/product/{id}
        [Authorize(Roles = "Admin, Seller")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteProduct(int id, [FromHeader] string username)
        {
            var deletedProduct = "";
            var tokenRole = User.FindFirst(ClaimTypes.Role)?.Value;
            if (tokenRole == "Admin")
            {
                deletedProduct = await _productService.DeleteProductAsync(id);
                _cacheHelper.RemoveKeys(ALL_PRODUCTS_CACHE_KEY, AVAILABLE_PRODUCTS_PAGE_CACHE_KEY);
                return Ok(new ProductDelete { message = deletedProduct });
            }
            deletedProduct = await _productService.DeleteProductByEmployeeAsync(id, username);
            _cacheHelper.RemoveKeys(ALL_PRODUCTS_CACHE_KEY, AVAILABLE_PRODUCTS_PAGE_CACHE_KEY);
            return Ok(new ProductDelete { message = deletedProduct });
        }
    }
}