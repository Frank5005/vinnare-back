using Shared.DTOs;
using Data.Entities;

namespace Services.Interfaces
{
    public interface IProductService
    {
        Task<IEnumerable<ProductDto>> GetAllProductsAsync();
        Task<IEnumerable<ProductView>> GetAvailableProductsAsync();
        Task<ProductDto?> GetProductByIdAsync(int id);
        Task<Product> CreateProductAsync(ProductRequest productDto, string? tokenRole);

        Task<Product> UpdateProductAsync(int id, ProductUpdate productDto);

        Task<string> DeleteProductAsync(int id, string? tokenRole);
    }
}
