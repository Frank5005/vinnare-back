using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;
using Shared.DTOs;

namespace Api.Controllers
{
    [Route("api/coupons")]
    [ApiController]
    public class CouponController : ControllerBase
    {
        private readonly ICouponService _couponService;

        public CouponController(ICouponService couponService)
        {
            _couponService = couponService;
        }

        // GET: api/coupons
        [HttpGet]
        public async Task<IActionResult> GetAllCoupons()
        {
            var coupons = await _couponService.GetAllCouponsAsync();
            return Ok(coupons);
        }

        // GET: api/coupons/{id}
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetCouponById(int id)
        {
            var coupon = await _couponService.GetCouponByIdAsync(id);
            if (coupon == null) return NotFound();
            return Ok(coupon);
        }

        // POST: api/coupons
        [HttpPost]
        public async Task<IActionResult> CreateCoupon([FromBody] CouponDto couponDto)
        {
            if (couponDto == null) return BadRequest("Coupon data is required.");

            var createdCoupon = await _couponService.CreateCouponAsync(couponDto);
            return CreatedAtAction(nameof(GetCouponById), new { id = createdCoupon.Id }, createdCoupon);
        }

        // UPDATE: api/coupons/{id}
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateCoupon(int id, [FromBody] CouponDto couponDto)
        {
            if (couponDto == null) return BadRequest("Coupon data is required.");

            var updatedCoupon = await _couponService.UpdateCouponAsync(id, couponDto);
            if (updatedCoupon == null) return NotFound();
            return Ok(updatedCoupon);
        }

        // DELETE: api/coupons/{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteCoupon(int id)
        {
            var deletedCoupon = await _couponService.DeleteCouponAsync(id);
            if (deletedCoupon == null) return NotFound();
            return Ok(deletedCoupon);
        }
    }
}
