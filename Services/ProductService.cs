using Data;
using Data.Entities;
using Microsoft.EntityFrameworkCore;
using Services.Interfaces;
using Shared.DTOs;

namespace Services
{
    public class ProductService : IProductService
    {
        private readonly VinnareDbContext _context;

        public ProductService(VinnareDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ProductDto>> GetAllProductsAsync()
        {
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
    }
}
