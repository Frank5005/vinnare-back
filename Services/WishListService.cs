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

        public async Task<IEnumerable<WishListDto>> GetAllWishListsAsync()
        {
            return await _context.WishLists
                .Select(w => new WishListDto
                {
                    Id = w.Id,
                    UserId = w.UserId,
                    ProductId = w.ProductId
                })
                .ToListAsync();
        }

        public async Task<WishListDto?> GetWishListByIdAsync(int id)
        {
            var wishList = await _context.WishLists.FindAsync(id);
            if (wishList == null) return null;

            return new WishListDto
            {
                Id = wishList.Id,
                UserId = wishList.UserId,
                ProductId = wishList.ProductId
            };
        }

        public async Task<WishListDto> CreateWishListAsync(WishListDto wishListDto)
        {
            var wishList = new WishList
            {
                UserId = wishListDto.UserId,
                ProductId = wishListDto.ProductId
            };

            _context.WishLists.Add(wishList);
            await _context.SaveChangesAsync();

            return new WishListDto
            {
                Id = wishList.Id,
                UserId = wishList.UserId,
                ProductId = wishList.ProductId
            };
        }

        public async Task<WishListDto?> UpdateWishListAsync(int id, WishListDto wishListDto)
        {
            var wishList = await _context.WishLists.FindAsync(id);
            if (wishList == null) return null;

            wishList.UserId = wishListDto.UserId;
            wishList.ProductId = wishListDto.ProductId;

            await _context.SaveChangesAsync();

            return new WishListDto
            {
                Id = wishList.Id,
                UserId = wishList.UserId,
                ProductId = wishList.ProductId
            };
        }

        public async Task<WishListDto?> DeleteWishListAsync(int id)
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