using Data;
using Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Services.Interfaces;
using Shared.DTOs;

namespace Services
{
    public class CartService : ICartService
    {
        private readonly VinnareDbContext _context;
        private readonly ILogger<CartService> _logger;

        public CartService(VinnareDbContext context, ILogger<CartService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<CartDto>> GetAllCartsAsync()
        {
            return await _context.Carts
                .Select(c => new CartDto
                {
                    Id = c.Id,
                    UserId = c.UserId,
                    ProductId = c.ProductId,
                    Quantity = c.Quantity
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<CartDto>?> GetCartByUserId(Guid id)
        {
            var cart = await _context.Carts
                .Where(c => c.UserId == id)
               .Select(c => new CartDto
               {
                   Id = c.Id,
                   UserId = c.UserId,
                   ProductId = c.ProductId,
                   Quantity = c.Quantity
               })
               .ToListAsync();
            return cart;
        }

        public async Task<CartDto?> GetCartByUserId_ProductId(Guid user_id, int product_id)
        {
            var cart = await _context.Carts
                .Where(c => c.UserId == user_id && c.ProductId == product_id)
               .Select(c => new CartDto
               {
                   Id = c.Id,
                   UserId = c.UserId,
                   ProductId = c.ProductId,
                   Quantity = c.Quantity
               })
               .FirstOrDefaultAsync();
            return cart;
        }
        public async Task<CartDto> CreateCartAsync(CartDto cartDto)
        {
            var cart = new Cart
            {
                UserId = cartDto.UserId,
                ProductId = cartDto.ProductId,
                Quantity = cartDto.Quantity
            };

            _context.Carts.Add(cart);
            await _context.SaveChangesAsync();

            return new CartDto
            {
                Id = cart.Id,
                UserId = cart.UserId,
                ProductId = cart.ProductId,
                Quantity = cart.Quantity
            };
        }

        public async Task<CartDto?> UpdateCartQuantity(int id, int quantity)
        {
            var cart = await _context.Carts.FindAsync(id);
            if (cart == null) return null;

            cart.Quantity = quantity;

            await _context.SaveChangesAsync();

            return new CartDto
            {
                Id = cart.Id,
                UserId = cart.UserId,
                ProductId = cart.ProductId,
                Quantity = cart.Quantity
            };
        }

        public async Task<CartDto?> DeleteCartAsync(int id)
        {
            var cart = await _context.Carts.FindAsync(id);
            if (cart == null) return null;

            _context.Carts.Remove(cart);
            await _context.SaveChangesAsync();

            return new CartDto
            {
                Id = cart.Id,
                UserId = cart.UserId,
                ProductId = cart.ProductId,
                Quantity = cart.Quantity
            };
        }
    }
}