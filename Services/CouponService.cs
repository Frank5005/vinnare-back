using Data;
using Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Services.Interfaces;
using Shared.DTOs;

namespace Services
{
    public class CouponService : ICouponService
    {
        private readonly VinnareDbContext _context;
        private readonly ILogger<CouponService> _logger;

        public CouponService(VinnareDbContext context, ILogger<CouponService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<CouponDto>> GetAllCouponsAsync()
        {
            _logger.LogInformation("TESING");
            return await _context.Coupons
                .Select(c => new CouponDto
                {
                    Id = c.Id,
                    Code = c.Code,
                    DiscountPercentage = c.DiscountPercentage
                })
                .ToListAsync();
        }

        public async Task<CouponDto?> GetCouponByIdAsync(int id)
        {
            var coupon = await _context.Coupons.FindAsync(id);
            if (coupon == null) return null;

            return new CouponDto
            {
                Id = coupon.Id,
                Code = coupon.Code,
                DiscountPercentage = coupon.DiscountPercentage
            };
        }

        public async Task<CouponDto> CreateCouponAsync(CouponDto couponDto)
        {
            var coupon = new Coupon
            {
                Code = couponDto.Code,
                DiscountPercentage = couponDto.DiscountPercentage
            };

            _context.Coupons.Add(coupon);
            await _context.SaveChangesAsync();

            return new CouponDto
            {
                Id = coupon.Id,
                Code = coupon.Code,
                DiscountPercentage = coupon.DiscountPercentage
            };
        }

        public async Task<CouponDto?> UpdateCouponAsync(int id, CouponDto couponDto)
        {
            var coupon = await _context.Coupons.FindAsync(id);
            if (coupon == null) return null;

            coupon.Code = couponDto.Code;
            coupon.DiscountPercentage = couponDto.DiscountPercentage;

            await _context.SaveChangesAsync();

            return new CouponDto
            {
                Id = coupon.Id,
                Code = coupon.Code,
                DiscountPercentage = coupon.DiscountPercentage
            };
        }

        public async Task<CouponDto?> DeleteCouponAsync(int id)
        {
            var coupon = await _context.Coupons.FindAsync(id);
            if (coupon == null) return null;

            _context.Coupons.Remove(coupon);
            await _context.SaveChangesAsync();

            return new CouponDto
            {
                Id = coupon.Id,
                Code = coupon.Code,
                DiscountPercentage = coupon.DiscountPercentage
            };
        }
    }
}