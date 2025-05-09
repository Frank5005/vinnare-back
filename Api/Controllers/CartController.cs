using System.Security.Claims;
using Api.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;
using Shared.DTOs;
using Shared.Exceptions;
using Swashbuckle.AspNetCore.Annotations;

namespace Api.Controllers
{
    [Route("api/cart")]
    [ApiController]
    [Authorize(Roles = "Shopper")]
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;
        private readonly IUserService _userService;
        private readonly IProductService _productService;

        public CartController(ICartService cartService, IUserService userService, IProductService productService)
        {
            _cartService = cartService;
            _userService = userService;
            _productService = productService;
        }


        // GET: api/cart/full
        [HttpGet("full")]
        public async Task<IActionResult> GetFullCartById()
        {
            var tokenUsername = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userId = await _userService.GetIdByEmail(tokenUsername) ?? throw new BadRequestException("User does not exist");

            var cart = await _cartService.GetFullCartByUserId(userId);
            if (cart == null) { throw new NotFoundException("Your cart is empty"); }



            return Ok(cart);
        }

        // GET: api/cart
        [HttpGet]
        public async Task<IActionResult> GetCartById()
        {
            var tokenUsername = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userId = await _userService.GetIdByEmail(tokenUsername) ?? throw new BadRequestException("User does not exist");

            var cart = await _cartService.GetCartByUserId(userId);
            if (cart == null) { throw new NotFoundException("Your cart is empty"); }
            return Ok(cart);
        }

        // POST: api/cart
         [HttpPost]
        public async Task<IActionResult> CreateCart([FromBody] CreateCartRequest cartRequest)
        {
            if (cartRequest == null) { throw new BadRequestException("Cart data is required."); }

            var tokenUsername = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userId = await _userService.GetIdByEmail(tokenUsername) ?? throw new BadRequestException("User does not exist");
            var product = await _productService.GetProductForCartWishByIdAsync(cartRequest.productId);

            if (product == null)
            {
                throw new NotFoundException("The product does not exists");
            }
            if (!product.Approved)
            {
                throw new GoneException("The product isn't approved");
            }
            if (product.Available <= 0)
            {
                throw new GoneException("No available products on stock");
            };
            if (cartRequest.quantity > product.Available)
            {
                cartRequest.quantity = product.Available;
            }
            var cartDto = new CartDto
            {
                ProductId = product.Id,
                Quantity = cartRequest.quantity,
                UserId = userId
            };
            var createdCart = await _cartService.CreateCartAsync(cartDto);
            return Created("", createdCart);
        }

        // UPDATE: api/cart/{product_id}
        [HttpPut("{product_id:int}")]
        public async Task<IActionResult> UpdateCart(int product_id, [FromQuery] int quantity)
        {
            var cartElement = await getCartItem(product_id);
            var product = await _productService.GetProductByIdAsync(product_id);
            if (product == null)
            {
                throw new GoneException("Products is not available");
            }
            if (quantity > product.Available)
            {
                quantity = product.Available;
            }

            var updatedCart = await _cartService.UpdateCartQuantity(cartElement.Id, quantity);
            if (updatedCart == null) { throw new BadRequestException("No item was modified"); };
            return Ok(new DefaultResponse { message = "ammount modified from cart" });
        }

        // DELETE: api/cart/{product_id}
        [HttpDelete("{product_id:int}")]
        public async Task<IActionResult> DeleteCart(int product_id)
        {

            var cartElement = await getCartItem(product_id);

            var deletedCart = await _cartService.DeleteCartAsync(cartElement.Id);
            if (deletedCart == null) { throw new BadRequestException("No item was deleted"); };
            return Ok(new DefaultResponse { message = "Item removed from cart" });
        }


        [SwaggerIgnore]
        public async Task<CartDto> getCartItem(int product_id)
        {
            var tokenUsername = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userId = await _userService.GetIdByUsername(tokenUsername) ?? throw new BadRequestException("User does not exist");
            var cartElement = await _cartService.GetCartByUserId_ProductId(userId, product_id) ?? throw new NotFoundException("Cart element does not exist");
            return cartElement;
        }
    }
}
