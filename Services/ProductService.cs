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
                    OwnerId = p.OwnerId,
                    Title = p.Title,
                    Price = p.Price,
                    Category = p.Category,
                    Approved = p.Approved
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
                OwnerId = product.OwnerId,
                Title = product.Title,
                Price = product.Price,
                Category = product.Category,
                Approved = product.Approved
            };
        }

        public async Task<ProductDto> CreateProductAsync(ProductDto productDto)
        {
            var product = new Product
            {
                OwnerId = productDto.OwnerId,
                Title = productDto.Title,
                Price = productDto.Price,
                Category = productDto.Category,
                Approved = productDto.Approved
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return productDto;
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
                Approved = product.Approved
            };
        }
    }
}
