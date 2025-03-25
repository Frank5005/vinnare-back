using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;
using Services.Interfaces;
using Shared.DTOs;
using Shared.Exceptions;

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

        // GET: api/product/all
        [Authorize(Roles = "Admin, Seller")]
        [HttpGet("all")]
        public async Task<IActionResult> GetAllProducts()
        {
            var products = await _productService.GetAllProductsAsync();
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
            var products = await _productService.GetAvailableProductsPageAsync();
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
            if(tokenRole == "Admin")
            {
                var createdProduct = await _productService.CreateProductAsync(productDto);
                return Ok(new ProductResponse { Id = createdProduct.Id, message = "Product created successfully" });
            }
            else
            {
                var createdProduct = await _productService.CreateProductByEmployeeAsync(productDto);
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
                return Ok(new ProductDelete { message = deletedProduct });
            }
            deletedProduct = await _productService.DeleteProductByEmployeeAsync(id, username);
            return Ok(new ProductDelete { message = deletedProduct});
        }
    }
}
