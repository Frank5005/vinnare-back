using Shared.DTOs;

namespace Services.Interfaces
{
    public interface ICartService
    {
        Task<IEnumerable<CartDto>> GetAllCartsAsync();
        Task<CartDto?> GetCartByIdAsync(int id);
        Task<CartDto> CreateCartAsync(CartDto cartDto);

        Task<CartDto?> UpdateCartAsync(int id, CartDto cartDto);

        Task<CartDto?> DeleteCartAsync(int id);
    }
}
