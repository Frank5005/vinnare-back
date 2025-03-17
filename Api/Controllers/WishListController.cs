using Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;
using Shared.DTOs;

namespace Api.Controllers
{
    [Route("api/wishlists")]
    [ApiController]
    public class WishListController : ControllerBase
    {
        private readonly IWishListService _wishListService;

        public WishListController(IWishListService wishListService)
        {
            _wishListService = wishListService;
        }

        // GET: api/wishlists
        [HttpGet]
        public async Task<IActionResult> GetAllWishLists()
        {
            var wishLists = await _wishListService.GetAllWishListsAsync();
            return Ok(wishLists);
        }

        // GET: api/wishlists/{id}
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetWishListById(int id)
        {
            var wishList = await _wishListService.GetWishListByIdAsync(id);
            if (wishList == null) return NotFound();
            return Ok(wishList);
        }

        // POST: api/wishlists
        [HttpPost]
        public async Task<IActionResult> CreateWishList([FromBody] WishListDto wishListDto)
        {
            if (wishListDto == null) return BadRequest("WishList data is required.");

            var createdWishList = await _wishListService.CreateWishListAsync(wishListDto);
            return CreatedAtAction(nameof(GetWishListById), new { id = createdWishList.Id }, createdWishList);
        }

        // UPDATE: api/wishlists/{id}
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateWishList(int id, [FromBody] WishListDto wishListDto)
        {
            if (wishListDto == null) return BadRequest("WishList data is required.");

            var updatedWishList = await _wishListService.UpdateWishListAsync(id, wishListDto);
            if (updatedWishList == null) return NotFound();
            return Ok(updatedWishList);
        }

        // DELETE: api/wishlists/{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteWishList(int id)
        {
            var deletedWishList = await _wishListService.DeleteWishListAsync(id);
            if (deletedWishList == null) return NotFound();
            return Ok(deletedWishList);
        }
    }
}
