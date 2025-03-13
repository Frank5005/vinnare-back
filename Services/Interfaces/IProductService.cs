using Shared.DTOs;

namespace Services.Interfaces
{
    public interface IProductService
    {
        Task<IEnumerable<ProductDto>> GetAllProductsAsync();
        Task<ProductDto?> GetProductByIdAsync(int id);
        Task<ProductDto> CreateProductAsync(ProductDto productDto);

        Task<ProductDto?> UpdateProductAsync(int id, ProductDto productDto);

        Task<ProductDto?> DeleteProductAsync(int id);
    }
}
