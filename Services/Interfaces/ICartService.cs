using Shared.DTOs;

namespace Services.Interfaces
{
    public interface ICartService
    {
        Task<IEnumerable<CartItemDto>> GetFullCartByUserId(Guid userId);
        Task<IEnumerable<CartDto>?> GetCartByUserId(Guid id);
        Task<CartDto?> GetCartByUserId_ProductId(Guid user_id, int product_id);
        Task<CartDto> CreateCartAsync(CartDto cartDto);

        Task<CartDto?> UpdateCartQuantity(int id, int quantity);

        Task<CartDto?> DeleteCartAsync(int id);
    }
}
