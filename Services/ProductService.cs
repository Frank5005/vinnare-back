using Data;
using Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Services.Interfaces;
using Shared.DTOs;

namespace Services
{
    public class ProductService : IProductService
    {
        private readonly VinnareDbContext _context;
        private readonly ILogger<ProductService> _logger;

        public ProductService(VinnareDbContext context, ILogger<ProductService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<ProductDto>> GetAllProductsAsync()
        {
            _logger.LogInformation("TESING");
            return await _context.Products
                .Select(p => new ProductDto
                {
                    Id = p.Id,
                    Title = p.Title,
                    Price = p.Price,
                    Category = p.Category
                })
                .ToListAsync();
        }


        public async Task<IEnumerable<ProductDto>> GetAvailableProductsAsync()
        {
            return await _context.Products
                .Where(p => p.Available > 0)
                .Select(p => new ProductDto
                {
                    Id = p.Id,
                    Title = p.Title,
                    Price = p.Price,
                    Category = p.Category
                })
                .ToListAsync();
        }


        public async Task<ProductDto?> GetProductByIdAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return null;

            return new ProductDto
            {
                Id = product.Id,
                Title = product.Title,
                Price = product.Price,
                Category = product.Category
            };
        }

        public async Task<Product> CreateProductAsync(ProductRequest productDto, string? tokenRole)
        {

            // Verify if the user exists
            var userExists = await _context.Users.AnyAsync(u => u.Id == productDto.OwnerId);
            if (!userExists)
            {
                throw new Exception("The owner doesn't exists.");
            }

            var product = new Product
            {
                OwnerId = productDto.OwnerId,
                Title = productDto.Title,
                Price = productDto.Price,
                Category = productDto.Category,
                Approved = tokenRole == "Admin",
                Description = productDto.Description,
                Image = productDto.Image,
                Quantity = productDto.Quantity,
                Available = productDto.Available
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return product;
        }

        public async Task<ProductDto?> UpdateProductAsync(int id, ProductDto productDto)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return null;

            product.OwnerId = productDto.OwnerId;
            product.Title = productDto.Title;
            product.Price = productDto.Price;
            product.Category = productDto.Category;
            product.Approved = productDto.Approved;
            product.Description = productDto.Description;
            product.Image = productDto.Image;
            product.Quantity = productDto.Quantity;
            product.Available = productDto.Available;

            await _context.SaveChangesAsync();

            return productDto;
        }

        public async Task<ProductDto?> DeleteProductAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return null;

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return new ProductDto
            {
                Id = product.Id,
                OwnerId = product.OwnerId,
                Title = product.Title,
                Price = product.Price,
                Category = product.Category,
                Approved = product.Approved,
                Description = product.Description,
                Image = product.Image,
                Quantity = product.Quantity,
                Available = product.Available
            };
        }

        public async Task ApproveProduct(int productId, bool approve)
        {

            var product = await _context.Products.FindAsync(productId);
            product!.Approved = approve;
            await _context.SaveChangesAsync();

        }
    }
}
