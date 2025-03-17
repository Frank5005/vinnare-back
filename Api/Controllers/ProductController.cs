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
        [HttpGet]
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

        // POST: api/product
        [HttpPost]
        public async Task<IActionResult> CreateProduct([FromBody] ProductDto productDto)
        {
            if (productDto == null) return BadRequest("Product data is required.");

            var createdProduct = await _productService.CreateProductAsync(productDto);
            return CreatedAtAction(nameof(GetProductById), new { id = createdProduct.Id }, createdProduct);
        }

        // UPDATE: api/product/{id}
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] ProductDto productDto)
        {
            if (productDto == null) return BadRequest("Product data is required.");

            var updatedProduct = await _productService.UpdateProductAsync(id, productDto);
            if (updatedProduct == null) return NotFound();
            return Ok(updatedProduct);
        }

        // DELETE: api/product/{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var deletedProduct = await _productService.DeleteProductAsync(id);
            if (deletedProduct == null) return NotFound();
            return Ok(deletedProduct);
        }
    }
}
