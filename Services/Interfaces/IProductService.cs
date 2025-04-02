using Data.Entities;
using Shared.DTOs;

namespace Services.Interfaces
{
    public interface IProductService
    {
        Task<IEnumerable<ProductDto>> GetAllProductsAsync();
        //Task<IEnumerable<ProductView>> GetAvailableProductsAsync();
        Task<IEnumerable<ProductViewPage>> GetAvailableProductsPageAsync();
        Task<IEnumerable<ProductViewPage>> GetProductsByCategoryAsync(int id);
        Task<ProductDetail> GetProductByIdAsync(int id);
        Task<string> GetProductNameByIdAsync(int id);
        Task<ProductDto?> GetProductForCartWishByIdAsync(int id);
        Task<Product> CreateProductAsync(ProductRequest productDto);
        Task<Product> CreateProductByEmployeeAsync(ProductRequest productDto);

        Task<Product> UpdateProductAsync(int id, ProductUpdate productDto);

        Task<string> DeleteProductAsync(int id);
        Task<string> DeleteProductByEmployeeAsync(int id, string username);
        Task ApproveProduct(int productId, bool approve);
    }
}
