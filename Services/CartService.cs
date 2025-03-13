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

        public async Task<CartDto?> GetCartByIdAsync(int id)
        {
            var cart = await _context.Carts.FindAsync(id);
            if (cart == null) return null;

            return new CartDto
            {
                Id = cart.Id,
                UserId = cart.UserId,
                ProductId = cart.ProductId,
                Quantity = cart.Quantity
            };
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

        public async Task<CartDto?> UpdateCartAsync(int id, CartDto cartDto)
        {
            var cart = await _context.Carts.FindAsync(id);
            if (cart == null) return null;

            cart.UserId = cartDto.UserId;
            cart.ProductId = cartDto.ProductId;
            cart.Quantity = cartDto.Quantity;

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