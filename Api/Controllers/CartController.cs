using Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;
using Shared.DTOs;

namespace Api.Controllers
{
    [Route("api/wishlists")]
    [ApiController]
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;

        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        // GET: api/carts
        [HttpGet]
        public async Task<IActionResult> GetAllCarts()
        {
            var carts = await _cartService.GetAllCartsAsync();
            return Ok(carts);
        }

        // GET: api/carts/{id}
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetCartById(int id)
        {
            var cart = await _cartService.GetCartByIdAsync(id);
            if (cart == null) return NotFound();
            return Ok(cart);
        }

        // POST: api/carts
        [HttpPost]
        public async Task<IActionResult> CreateCart([FromBody] CartDto cartDto)
        {
            if (cartDto == null) return BadRequest("Cart data is required.");

            var createdCart = await _cartService.CreateCartAsync(cartDto);
            return CreatedAtAction(nameof(GetCartById), new { id = createdCart.Id }, createdCart);
        }

        // UPDATE: api/carts/{id}
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateCart(int id, [FromBody] CartDto cartDto)
        {
            if (cartDto == null) return BadRequest("Cart data is required.");

            var updatedCart = await _cartService.UpdateCartAsync(id, cartDto);
            if (updatedCart == null) return NotFound();
            return Ok(updatedCart);
        }

        // DELETE: api/carts/{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteCart(int id)
        {
            var deletedCart = await _cartService.DeleteCartAsync(id);
            if (deletedCart == null) return NotFound();
            return Ok(deletedCart);
        }
    }
}
