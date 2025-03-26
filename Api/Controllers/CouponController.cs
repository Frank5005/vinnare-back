using Api.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;
using Shared.DTOs;
using Shared.Exceptions;

namespace Api.Controllers
{
    [Route("api/coupons")]
    [ApiController]
    [Authorize(Roles = "Admin")]
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


        // POST: api/coupons
        [HttpPost]
        public async Task<IActionResult> CreateCoupon([FromBody] CouponRequest couponRequest)
        {
            if (couponRequest == null)
            {
                throw new BadRequestException("Coupon data is required.");
            }
            if (couponRequest.discountPercentage >= 100)
            {
                throw new BadRequestException("No free stuff!");
            }
            var couponDto = new CouponDto
            {
                Code = couponRequest.code,
                DiscountPercentage = couponRequest.discountPercentage
            };

            var createdCoupon = await _couponService.CreateCouponAsync(couponDto);
            return Created("", createdCoupon);
        }



        // DELETE: api/coupons/{id}
        [HttpDelete("{code:alpha}")]
        public async Task<IActionResult> DeleteCoupon(string code)
        {

            var coupon = await _couponService.GetCouponByCode(code);
            if (coupon == null)
            {
                throw new NotFoundException("that coupon does not exists");
            }
            var deletedCoupon = await _couponService.DeleteCouponAsync(coupon.Id);
            if (deletedCoupon == null)
            {
                throw new NotFoundException("that coupon does not exists");
            }
            return Ok(new DefaultResponse { message = "deleted coupon" });
        }
    }
}
