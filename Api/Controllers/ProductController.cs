using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;
using Shared.DTOs;

namespace Api.Controllers
{
    [Route("api/product")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        // GET: api/product
        [Authorize(Roles = "Admin, Seller")]
        [HttpGet("all")]
        public async Task<IActionResult> GetAllProducts()
        {
            var products = await _productService.GetAllProductsAsync();
            return Ok(products);
        }

        // GET: api/product/store
        [HttpGet("store")]
        public async Task<IActionResult> GetAvailableProducts()
        {
            var products = await _productService.GetAvailableProductsAsync();
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
            if (productDto == null) return BadRequest("Product data is required.");

            var tokenRole = User.FindFirst(ClaimTypes.Role)?.Value;
            var createdProduct = await _productService.CreateProductAsync(productDto, tokenRole);
            if (createdProduct.Approved == false)
            {
                return Ok(new ProductResponse { Id = createdProduct.Id, message = "Waiting to approve" });
            }
            if (createdProduct.Approved == true)
            {
                return Ok(new ProductResponse { Id = createdProduct.Id, message = "Product created successfully" });
            }
            return BadRequest("Product not created");
        }

        // UPDATE: api/product/{id}
        [Authorize(Roles = "Admin, Seller")]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] ProductUpdate productDto)
        {
            if (productDto == null) return BadRequest("Product data is required.");

            var updatedProduct = await _productService.UpdateProductAsync(id, productDto);
            if (updatedProduct == null) return NotFound();
            return Ok(new ProductResponse { Id = updatedProduct.Id, message = "Product updated successfully" });
        }

        // DELETE: api/product/{id}
        [Authorize(Roles = "Admin, Seller")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var tokenRole = User.FindFirst(ClaimTypes.Role)?.Value;
            var deletedProduct = await _productService.DeleteProductAsync(id);
            return Ok(new ProductDelete { message = "deletedProduct" });
        }
    }
}
