using Shared.DTOs;

namespace Services.Interfaces
{
    public interface ICouponService
    {
        Task<IEnumerable<CouponDto>> GetAllCouponsAsync();
        Task<CouponDto?> GetCouponByIdAsync(int id);
        Task<CouponDto> CreateCouponAsync(CouponDto couponDto);

        Task<CouponDto?> UpdateCouponAsync(int id, CouponDto couponDto);

        Task<CouponDto?> DeleteCouponAsync(int id);
    }
}
