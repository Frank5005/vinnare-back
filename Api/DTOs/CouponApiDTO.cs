namespace Api.DTOs
{
    public class CouponRequest
    {
        public string code { get; set; }
        public int discountPercentage { get; set; }
    }

    public class PurchaseRequest
    {
        public string? coupon_code { get; set; }
    }

}