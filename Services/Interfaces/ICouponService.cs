using Shared.DTOs;

namespace Services.Interfaces
{
    public interface ICouponService
    {
        Task<IEnumerable<CouponDto>> GetAllCouponsAsync();
        Task<CouponDto?> GetCouponByCode(string code);
        Task<CouponDto> CreateCouponAsync(CouponDto couponDto);
        Task<CouponDto?> DeleteCouponAsync(int id);
    }
}
