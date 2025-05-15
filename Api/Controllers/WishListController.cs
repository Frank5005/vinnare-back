using System.Security.Claims;
using Api.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;
using Shared.DTOs;
using Shared.Exceptions;

namespace Api.Controllers
{
    [Route("api/user/wishlist")]
    [ApiController]
    public class WishListController : ControllerBase
    {
        private readonly IWishListService _wishListService;
        private readonly IUserService _userService;
        private readonly IProductService _productService;

        public WishListController(IWishListService wishListService, IUserService userService, IProductService productService)
        {
            _wishListService = wishListService;
            _userService = userService;
            _productService = productService;
        }

        // GET: api/user/wishlist
        [Authorize(Roles = "Shopper")]
        [HttpGet]
        public async Task<IActionResult> GetAllWishLists()
        {
            var tokenUsername = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user_id = await _userService.GetIdByEmail(tokenUsername) ?? throw new NotFoundException("No user found with that email.");
            var wishLists = await _wishListService.GetAllWishListsAsync(user_id);

            var products = new List<ProductDto>();
            foreach (var id in wishLists)
            {
                var product = await _productService.GetProductForCartWishByIdAsync(id);
                if (product != null && product.Approved)
                {
                    products.Add(product);
                }
            }

            //return Ok(new GetWishListResponse { Products = products });
            return Ok(products);
        }

        // POST: api/user/wishlist/add/{product_id}
        [Authorize(Roles = "Shopper")]
        [HttpPost("add")]
        public async Task<IActionResult> CreateWishList([FromBody] CreateWishListRequest wishListRequest)
        {
            var tokenUsername = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var user_id = await _userService.GetIdByEmail(tokenUsername);
            if (user_id == null)
            {
                throw new NotFoundException("No user found with that username.");
            }
            ;
            if (wishListRequest.UserId != user_id)
            {
                throw new UnauthorizedException("You are not loged in as the user. You can't add to someone else wishlist.");
            }

            var product = await _productService.GetProductForCartWishByIdAsync(wishListRequest.ProductId);
            if (product == null || product.Approved == false)
            {
                throw new NotFoundException("No Product with that id exists");
            }
            await _wishListService.CreateWishListAsync(wishListRequest);

            return Ok(new DefaultResponse { message = "Successfully added" });
        }

        // DELETE: api/user/wishlist/remove/{product_id}
        [Authorize(Roles = "Shopper")]
        [HttpDelete("{product_id:int}")]
        public async Task<IActionResult> DeleteWishList(int product_id)
        {
            var tokenUsername = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var user_id = await _userService.GetIdByEmail(tokenUsername) ?? throw new NotFoundException("No user found with that username.");
            var wishList = await _wishListService.GetWishListByProductId(user_id, product_id);

            if (wishList == null)
            {
                throw new NotFoundException("You don't have this prodcut on your wishList");
            }
            var deletedWishList = await _wishListService.DeleteWishListById(wishList.Id);
            if (deletedWishList == null) { throw new NotFoundException("You don't have this prodcut on your wishList."); }
            ;
            return Ok(new DefaultResponse { message = "Successfully deleted" });
        }
    }
}
