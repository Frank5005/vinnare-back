using Shared.DTOs;

namespace Services.Interfaces
{
    public interface IWishListService
    {
        Task<IEnumerable<WishListDto>> GetAllWishListsAsync();
        Task<WishListDto?> GetWishListByIdAsync(int id);
        Task<WishListDto> CreateWishListAsync(WishListDto wishListDto);

        Task<WishListDto?> UpdateWishListAsync(int id, WishListDto wishListDto);

        Task<WishListDto?> DeleteWishListAsync(int id);
    }
}
