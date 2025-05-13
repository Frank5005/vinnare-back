using Data;
using Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Services.Interfaces;
using Shared.DTOs;

namespace Services
{
    public class WishListService : IWishListService
    {
        private readonly VinnareDbContext _context;
        private readonly ILogger<WishListService> _logger;

        public WishListService(VinnareDbContext context, ILogger<WishListService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<int>> GetAllWishListsAsync(Guid userId)
        {
            return await _context.WishLists
                .Where(w => w.UserId == userId)
                .Select(w => w.ProductId)
                .ToListAsync();
        }

        public async Task<WishListDto?> GetWishListByProductId(Guid userId, int productId)
        {
            var wishlistQuery = from n in _context.WishLists
                                where n.UserId == userId && n.ProductId == productId
                                select n;

            var wishList = await wishlistQuery.FirstOrDefaultAsync();
            if (wishList == null) return null;

            return new WishListDto
            {
                Id = wishList.Id,
                UserId = wishList.UserId,
                ProductId = wishList.ProductId
            };
        }

        public async Task<WishListDto> CreateWishListAsync(CreateWishListRequest wishListRequest, Guid userId)
        {
            var existingWishList = await GetWishListByProductId(userId, wishListRequest.ProductId);
            if (existingWishList != null)
            {
                _logger.LogWarning("WishList already exists for product ID {ProductId}", wishListRequest.ProductId);
                return existingWishList;
            }

            var wishList = new WishList
            {
                //UserId = wishListRequest.UserId,
                ProductId = wishListRequest.ProductId
            };

            _context.WishLists.Add(wishList);
            await _context.SaveChangesAsync();

            return new WishListDto
            {
                Id = wishList.Id,
                UserId = userId,
                ProductId = wishList.ProductId
            };
        }

        public async Task<WishListDto?> DeleteWishListById(int id)
        {
            var wishList = await _context.WishLists.FindAsync(id);
            if (wishList == null) return null;

            _context.WishLists.Remove(wishList);
            await _context.SaveChangesAsync();

            return new WishListDto
            {
                Id = wishList.Id,
                UserId = wishList.UserId,
                ProductId = wishList.ProductId
            };
        }
    }
}