using Shared.DTOs;

namespace Services.Interfaces
{
    public interface IWishListService
    {
        Task<IEnumerable<int>> GetAllWishListsAsync(Guid userId);
        Task<WishListDto> CreateWishListAsync(CreateWishListRequest wishListDto, Guid userId);
        Task<WishListDto?> GetWishListByProductId(Guid userId, int productId);
        Task<WishListDto?> DeleteWishListById(int id);
    }
}
